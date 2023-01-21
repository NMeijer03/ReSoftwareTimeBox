using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ReSoftwareTimeBox.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReSoftwareTimeBox.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonsController : ControllerBase
    {
        string connectionstring = "server=127.0.0.1;uid=root;database=Resoftware";

        //read
        [HttpGet]
        public ActionResult GetPersonModel()
        {
            //reads the database for all persons records
            string query = "SELECT * FROM Persons";
            List<PersonModel> PersonModelArr = new List<PersonModel>();

            using (MySqlConnection con = new MySqlConnection(connectionstring))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    con.Open();

                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        PersonModel PersonModel = new PersonModel();
                        PersonModel.PersonId = (int)reader["PersonId"];
                        PersonModel.FirstName = (string)reader["FirstName"];
                        PersonModel.Surname = (string)reader["Surname"];
                        PersonModel.Email = (string)reader["Email"];
                        PersonModel.Age = (int)reader["Age"];

                        PersonModelArr.Add(PersonModel);
                    }
                }
            }
            //if nothing could be read return error code 404
            if (!PersonModelArr.Any())
                return NotFound();
            return Ok(PersonModelArr);
        }

        //create
        [HttpPost]
        public ActionResult CreatePersonModel(string firstname, string surname, string email, int age)
        {
            PersonModel newPerson = new PersonModel();

            //validate information
            bool badThingsDidntHappened = false;

            string query = @"INSERT INTO Persons(FirstName, Surname, Email, Age) VALUES(@FirstName, @Surname, @Email, @Age);";
            using (MySqlConnection con = new MySqlConnection(connectionstring))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("FirstName", firstname);
                    cmd.Parameters.AddWithValue("Surname", surname);
                    cmd.Parameters.AddWithValue("Email", email);
                    cmd.Parameters.AddWithValue("Age", age);

                    newPerson.FirstName = firstname;
                    newPerson.Surname = surname;
                    newPerson.Email = email;
                    newPerson.Age = age;

                    //checks if email is valid
                    var trimmedEmail = newPerson.Email.Trim();

                    if (trimmedEmail.EndsWith("."))
                    {
                        return BadRequest("One or more fields are invalid");
                    }
                    try
                    {
                        var addr = new System.Net.Mail.MailAddress(newPerson.Email);
                        badThingsDidntHappened = addr.Address == trimmedEmail;
                    }
                    catch
                    {
                        return BadRequest("One or more fields are invalid");
                    }

                    con.Open();

                    if (cmd.ExecuteNonQuery() == 0)
                        badThingsDidntHappened = true;
                }
            }

            if (badThingsDidntHappened != true)
                return BadRequest("One or more fields are invalid");
            return Created("", newPerson);
        }

        //delete
        [HttpDelete("{id}")] //PersonModel/{id}
        public ActionResult DeletePersonModel(int id)
        {
            bool badThingsHappened = false;

            string query = @"DELETE FROM Persons WHERE PersonId = @Id;";
            using (MySqlConnection con = new MySqlConnection(connectionstring))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("Id", id);

                    con.Open();

                    if (cmd.ExecuteNonQuery() == 0)
                        badThingsHappened = true;
                }
            }

            if (badThingsHappened == true)
                return BadRequest();

            return NoContent();
        }
    }
}
