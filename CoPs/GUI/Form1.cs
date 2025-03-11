using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using GUI.Clases;

namespace GUI
{
    public partial class Form1 : Form
    {
        private string scannedCode = "";
        private RegistroCodigos registroCodigos;

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyPress += ScannerForm_KeyPress;

            registroCodigos = new RegistroCodigos("codigos.json");
            this.FormClosing += Form1_FormClosing;
            this.CenterToScreen();
            //generarCodigos();
            ActualizarGrilla();
        }

        private void ScannerForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                ValidateEAN13WithSteps(scannedCode);

                registroCodigos.AgregarCodigo(scannedCode);
                scannedCode = ""; // Resetear para el próximo código
                textBox1.Text = scannedCode;

                ActualizarGrilla();
            }
            else
            {
                scannedCode += e.KeyChar; // Construir el código
            }
        }



        private void ActualizarGrilla()
        {
            dataGridView1.Rows.Clear();

            var codigosOrdenados = registroCodigos.Codigos
                .OrderByDescending(c => c.FechaUso)
                .ToList();

            // Contar cuántas veces ha sido usado cada código en total
            Dictionary<string, int> totalUsos = new Dictionary<string, int>();
            foreach (var item in codigosOrdenados)
            {
                if (!totalUsos.ContainsKey(item.CodigoValor))
                {
                    totalUsos[item.CodigoValor] = codigosOrdenados.Count(c => c.CodigoValor == item.CodigoValor) - 1;
                }
            }

            // Agregar filas a la grilla con el contador en orden descendente
            Dictionary<string, int> usosRestantes = new Dictionary<string, int>(totalUsos);
            foreach (var item in codigosOrdenados)
            {
                dataGridView1.Rows.Add(item.CodigoValor, item.FechaUso.ToString("dd/MM/yyyy HH:mm"), usosRestantes[item.CodigoValor]);

                usosRestantes[item.CodigoValor]--; // Disminuir el contador
            }
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            registroCodigos.GuardarEnJSON();
        }

        private bool ValidateEAN13WithSteps(string code)
        {
            try
            {
                // Variables para almacenar los mensajes de error
                List<string> errores = new List<string>();

                // Validar que el código tenga 13 dígitos numéricos
                if (code.Length != 13 || !code.All(char.IsDigit))
                {
                    errores.Add("Código no válido: Debe tener 13 dígitos numéricos.");
                }
                else
                {

                    // Realizar el cálculo de la suma de verificación
                    int sum = 0;
                    for (int i = 0; i < 12; i++)
                    {
                        int digit = int.Parse(code[i].ToString());
                        sum += (i % 2 == 0) ? digit : digit * 3;
                    }

                    int mod = sum % 10;
                    int expectedCheckDigit = (10 - mod) % 10;
                    int checkDigit = int.Parse(code[12].ToString());

                    // Verificar si el dígito de control es correcto
                    if (expectedCheckDigit != checkDigit)
                    {
                        errores.Add("Código no válido: El dígito de control no coincide.");
                    }
                }
                // Aquí ya tenemos un código válido, proceder con la verificación adicional
                int vecesUsado = registroCodigos.ObtenerVecesUsado(code);

                // Obtener la última fecha de uso
                DateTime? ultimaFechaUso = registroCodigos.Codigos
                    .Where(c => c.CodigoValor == code)
                    .OrderByDescending(c => c.FechaUso)
                    .Select(c => (DateTime?)c.FechaUso)
                    .FirstOrDefault();

                // Si el código ha sido usado antes, mostrar un mensaje
                if (vecesUsado > 0)
                {
                    string mensaje = vecesUsado == 1
                        ? $"Código ya registrado. Se ha usado {vecesUsado} vez antes."
                        : $"Código ya registrado. Se ha usado {vecesUsado} veces antes.";

                    if (ultimaFechaUso.HasValue)
                    {
                        // Formatear la fecha de la última vez que se usó el código
                        string fechaFormateada = ultimaFechaUso.Value.ToString("dddd d 'de' MMMM 'de' yyyy", new CultureInfo("es-ES"));
                        mensaje += $"\nÚltimo uso: {fechaFormateada}";
                    }

                    errores.Add(mensaje);
                }

                // Si se encontraron errores, lanzamos una excepción con todos los errores combinados
                if (errores.Any())
                {
                    throw new Exception(string.Join("\n", errores));
                }

                return true;
            }
            catch (Exception ex)
            {
                // Aquí puedes lanzar el mensaje con la excepción detectada
                MessageBox.Show($"Posible receta maliciosa detectada. Le recomendamos no realizar la venta de este psicotrópico.\n\nRazón de la detección:\n{ex.Message}", "ADVERTENCIA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }


        public void generarCodigos()
        {
            var random = new Random();
            int cantidad = 15000;
            int i = 0;

            while (i < cantidad)
            {
                long codigo = (long)(random.NextDouble() * (9999999999999 - 9000000000000)) + 9000000000000;
                registroCodigos.AgregarCodigo(codigo.ToString());
                i++;
            }
        }

    }
}
