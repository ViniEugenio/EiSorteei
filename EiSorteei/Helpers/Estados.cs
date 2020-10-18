using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EiSorteei.Helpers
{
    public class Estados
    {
        public string Name { get; set; }
        public string Initials { get; set; }

        public static List<Estados> GetAllStates()
        {
            List<Estados> States = new List<Estados>()
            {
                new Estados
                {
                    Name="Acre",
                    Initials="AC"
                },

                new Estados 
                {
                    Name="Alagoas",
                    Initials="AL"
                },

                new Estados
                {
                    Name="Amazonas",
                    Initials="AM"
                },

                new Estados
                {
                    Name="Amapá",
                    Initials="AP"
                },

                new Estados
                {
                    Name="Bahia",
                    Initials="BA"
                },

                new Estados
                {
                    Name="Ceará",
                    Initials="CE"
                },

                new Estados
                {
                    Name="Distrito Federal ",
                    Initials="DF"
                },

                new Estados
                {
                    Name="Espírito Santo",
                    Initials="ES"
                },

                new Estados
                {
                    Name="Goiás",
                    Initials="GO"
                },

                new Estados
                {
                    Name="Maranhão",
                    Initials="MA" +
                    ""
                },

                new Estados
                {
                    Name="Minas Gerais",
                    Initials="MG"
                },

                new Estados
                {
                    Name="Mato Grosso do Sul",
                    Initials="MS"
                },

                new Estados
                {
                    Name="Mato Grosso",
                    Initials="MT"
                },

                new Estados
                { 
                    Name="Pará",
                    Initials="PA"
                },

                new Estados
                {
                    Name="Paraíba",
                    Initials="PB"
                },

                new Estados
                {
                    Name="Pernambuco",
                    Initials="PE"
                },

                new Estados
                {
                    Name="Piauí",
                    Initials="PI"
                },

                new Estados
                {
                    Name="Paraná",
                    Initials="PR"
                },

                new Estados
                {
                    Name="Rio de Janeiro",
                    Initials="RJ"
                },

                new Estados
                {
                    Name="Rio Grande do Norte",
                    Initials="RN"
                },

                new Estados
                {
                    Name="Rondônia",
                    Initials="RO"
                },

                new Estados
                {
                    Name="Roraima",
                    Initials="RR"
                },

                new Estados
                {
                    Name="Rio Grande do Sul",
                    Initials="RS"
                },

                new Estados
                {
                    Name="Santa Catarina",
                    Initials="SC"
                },

                new Estados
                {
                    Name="Sergipe",
                    Initials="SE"
                },

                new Estados
                {
                    Name="São Paulo",
                    Initials="SP"
                },

                new Estados
                {
                    Name="Tocantins",
                    Initials="TO"
                }
            };

            return States;
        }
    }
}