using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.Clases
{
    public class EntradaRegistro
    {
        public string CodigoValor { get; set; }
        public DateTime FechaUso { get; set; }

        public EntradaRegistro(string codigoValor, DateTime fecha)
        {
            CodigoValor = codigoValor;
            FechaUso = fecha;
        }
    }
}
