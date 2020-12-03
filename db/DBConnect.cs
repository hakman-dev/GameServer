//Add MySql Library

using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

class DBConnect
{
    private MySqlConnection connection;
    private string server;
    private string database;
    private string uid;
    private string password;

    //Constructor
    public DBConnect()
    {
        Initialize();
    }

    //Initialize values
    private void Initialize()
    {
        server = "localhost";
        database = "lucky";
        uid = "fcr01";
        password = "jsi81UmsU71P";
        string connectionString;

        connectionString = "SERVER=" + server + ";" + "DATABASE=" +
                           database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

        connection = new MySqlConnection(connectionString);
    }

    //open connection to database
    private bool OpenConnection()
    {
        try
        {
            connection.Open();
            return true;
        }
        catch (MySqlException ex)
        {
            //When handling errors, you can your application's response based 
            //on the error number.
            //The two most common error numbers when connecting are as follows:
            //0: Cannot connect to server.
            //1045: Invalid user name and/or password.
            switch (ex.Number)
            {
                case 0:
                    Console.WriteLine("Cannot connect to server.  Contact administrator");
                    // MessageBox.Show("");
                    break;

                case 1045:
                    Console.WriteLine("Invalid username/password, please try again");
                    break;
            }

            return false;
        }
    }

    //Close connection
    private bool CloseConnection()
    {
        try
        {
            connection.Close();
            return true;
        }
        catch (MySqlException ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    //
    // //Insert statement
//Select statement
    public List< string >[] Games()
    {
        string query = "SELECT * FROM games";

        //Create a list to store the result
        List< string >[] list = new List< string >[6];
        list[0] = new List< string >(); // id
        list[1] = new List< string >(); // start_at
        list[2] = new List< string >(); // price
        list[3] = new List< string >(); // iser_count
        list[4] = new List< string >(); // created_at
        list[5] = new List< string >(); // updated_at

        //Open connection
        if (this.OpenConnection() == true)
        {
            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();
        
            //Read the data and store them in the list
            while (dataReader.Read())
            {
                Console.WriteLine((dataReader["start_at"] + ""));
                list[0].Add(dataReader["id"] + "");
                list[1].Add(dataReader["start_at"] + "");
                list[2].Add(dataReader["price"] + "");
                list[3].Add(dataReader["user_count"] + "");
                list[4].Add(dataReader["created_at"] + "");
                list[5].Add(dataReader["updated_at"] + "");
                
            }

            //close Data Reader
            dataReader.Close();

            //close Connection
            this.CloseConnection();

            //return list to be displayed
            Console.WriteLine(list);
            return list;
        }
        else
        {
            Console.WriteLine(list);
            return list;
        }
    }
    //
    // //Update statement
    // public void Update()
    // {
    //     //
    // }
    //
    // //Delete statement
    // public void Delete()
    // {
    //     //
    // }
    //
    // //Select statement
    // public List <string> [] Select()
    // {
    //     //
    // }
    //
    // //Count statement
    // public int Count()
    // {
    //     //
    // }
    //
    // //Backup
    // public void Backup()
    // {
    //     //
    // }
    //
    // //Restore
    // public void Restore()
    // {
    //     //
    // }
}