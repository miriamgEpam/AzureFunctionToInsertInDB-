using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionInsertToSqlAzure
{

    public class Alumno {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Edad { get;  set; }

    }
    public static class InsertToDb
    {
        [FunctionName("InsertToDb")]
       public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
             HttpRequestMessage req,
            ILogger log )
        {
            log.LogInformation("Info received.");
            string jsonContent = await req.Content.ReadAsStringAsync();
            var alumno = JsonConvert.DeserializeObject<Alumno>(jsonContent);
            var cnnString = "Server=tcp:alumnosserverdb.database.windows.net,1433;Initial Catalog=AlumnosLic;Persist Security Info=False;User ID=alumnossa;Password=alumnos.123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            #region
            // update 
            try
            {
                using (var connection = new SqlConnection(cnnString))
                {
                    connection.Open();
                    connection.Execute("INSERT INTO [dbo].[Alumnos] ([NombreAlumno],[Apellidos],[Edad]) VALUES ('" + alumno.Nombre + "','"+alumno.Apellidos+"','"+alumno.Edad+"')");
                    connection.Close();
                }
                log.LogInformation("Log added to database successfully! alumno:" + alumno.Nombre);
            }
            catch (Exception e)
            {
                log.LogInformation("Log added to database bad!: " + e.Message);
            }
            #endregion
            // select 
            var alumnosList = new List<Alumno>();
            try
            {
                using (var connection = new SqlConnection(cnnString))
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand("Select * from dbo.Alumnos", connection);
                    SqlDataReader reader = cmd.ExecuteReader();
                   
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var nombre = reader.GetString(1);
                        var apellidos = reader.GetString(2);
                        var edad = reader.GetString(3);
                        alumnosList.Add(new Alumno { Id = id, Apellidos = apellidos, Edad = edad, Nombre = nombre });
                        log.LogInformation(" Extractedc alumnoid "+ id+ "nombre "+ nombre+ " apellidos "+ apellidos+ " edad"+ edad);

                    }
                    log.LogInformation("Select executed ");
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                log.LogInformation("Log selected to database bad!: " + e.Message);

            }
            var serializedAlumnosList = JsonConvert.SerializeObject(alumnosList);
            return req.CreateResponse(HttpStatusCode.OK, alumnosList); 

        }
    }
}
