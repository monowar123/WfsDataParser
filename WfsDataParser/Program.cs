using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WfsDataParser
{
    class Program
    {
        static void Main(string[] args)
        {
            string wfsAddress = "http://afnemers.ruimtelijkeplannen.nl/afnemers2012/services?REQUEST=GetFeature&service=WFS&version=2.0.0&typename=app:ProvinciaalPlangebied";
            //string wfsAddress = "http://bgtviewer.chalois.com:8080/geoserver/ACC1/ows?service=WFS&version=1.0.0&request=GetFeature&typeName=ACC1:hitbgt_test";
            string outputFile = @"D:\Test_value\wfs\abc.shp";

            string connString = "SERVER=localhost; Port=5432; Database=AO01; User id=postgres; Password=postgres; encoding=unicode";

            try
            {
                WfsClient client = new WfsClient(wfsAddress);
                //client.CreateShape(outputFile);
                client.InsertIntoDb(connString, "abc");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }


            Console.WriteLine("Done");

            Console.ReadKey();
        }
    }
}
