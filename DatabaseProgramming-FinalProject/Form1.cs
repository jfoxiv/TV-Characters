using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace DatabaseProgramming_FinalProject {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }// end publc Form1

        // set up necessary variables/structures here
        public SqlConnection con = new SqlConnection("Data Source=" + @"(Local)\SQLEXPRESS;" + 
            "Initial Catalog=ShowCharacters; Integrated Security=True");

        // Dictionaries for binding to comboboxen on "Delete" and "Modify" functions.
        private Dictionary<string, string> prodCoIndex = new Dictionary<string, string>();
        private Dictionary<string, string> actorIndex  = new Dictionary<string, string>();
        private Dictionary<string, string> charIndex   = new Dictionary<string, string>();
        private Dictionary<string, string> showIndex   = new Dictionary<string, string>();

        private bool tablesExist() {
            SqlCommand cmd = con.CreateCommand();

            try {
                con.Open();
                cmd.CommandText = "SELECT ActorID FROM Actor";
                SqlDataReader reader = cmd.ExecuteReader();

                if(reader.FieldCount > 0) {
                    return true;
                }
            } catch (Exception ex) {
                return false;
            }
            finally {
                con.Close();
            }
            return true;
        }

        private void Form1_Load(object sender, EventArgs e) {
            if (tablesExist()) {
                enableRecordTabs();
                updateIndices();
            } else {
                disableRecordTabs();
            }
 
        }// end Form1_Load

        private void buttonCreateTables_Click(object sender, EventArgs e) {
            bool sqlError = false;
            // try to create the tables--if they don't exist already
            if (tablesExist() == true) {
                MessageBox.Show("The tables already exist!");
            } else {

                SqlCommand cmd = con.CreateCommand();

                try {
                    con.Open();
                    // We're using FK constraints, so proper ordering is vital
                    // for creation and population.
                    cmd.CommandText = "CREATE TABLE Actor(" +
                        "ActorID INT IDENTITY(1,1) PRIMARY KEY, " +
                        "FName CHAR (30), " + 
                        "LName CHAR(100))";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "CREATE TABLE ProdCo(" +
                        "prodCoID INT IDENTITY(1,1) PRIMARY KEY, " +
                        "Name CHAR(30))";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "create table Show(" +
                        "ShowID int IDENTITY(1,1) PRIMARY KEY, " +
                        "Name CHAR(30), " +
                        "prodCoID INT NOT NULL REFERENCES ProdCo (ProdCoID))";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "CREATE TABLE Character( " +
                        "CharID INT IDENTITY(1,1) PRIMARY KEY, " + 
						"FName CHAR(30), LName CHAR(20), " +
						"ShowID INT NOT NULL REFERENCES Show (ShowID), " +
						"ActorID INT NOT NULL REFERENCES Actor (ActorID))";
                    cmd.ExecuteNonQuery();



                } catch (Exception ex) {
                    MessageBox.Show("Error trying to create a table: " + ex.Message);
                    sqlError = true;
                }
                finally {
                    con.Close();
                }

                // Okay, now let's load up some dummy data.
                // Note: parameterization seems unncessary here, given that
                // these commands are entirely hard-coded and thus there's
                // no chance for users to cause trouble.
                try {
                    con.Open();

                    // As mentioned previously, FK constraints mean we have to be
                    // careful in what order we populate these tables:
                    //  * Actor
                    //  * Production company
                    //  * Show
                    //  * Characters

                    cmd.CommandText = "insert into Actor values('Emma', 'Caulfield')";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "insert into Actor values('Melissa', 'O''Neil')";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "insert into Actor values('Anna', 'Torv')";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "insert into ProdCo values('Mutant Enemy')";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "insert into ProdCo values('Prodigy Pictures')";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "insert into ProdCo values('Bad Robot')";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "insert into Show values('Buffy the Vampire Slayer', 1)";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "insert into Show values('Dark Matter', 2)";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "insert into Show values('Fringe', 3)";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "insert into Character values('Anya', 'Jenkins', 1, 1)";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "insert into Character values('Portia', 'Lin', 2, 2)";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "insert into Character values('Olivia', 'Dunham', 3, 3)";
                    cmd.ExecuteNonQuery();

                } catch (Exception ex) {
                    MessageBox.Show("Error inserting data into table: " + ex.Message);
                    sqlError = true;
                } finally {
                    con.Close();
                }

                if (sqlError == false) {
                    MessageBox.Show("Tables created and loaded with data.");
                }

                updateIndices();
                enableRecordTabs();
            }

        } // end buttonCreateTables_Click 
        private void buttonDeleteTables_Click(object sender, EventArgs e) {
            // Drop tables, yo. Be sure to do it in opposite order from
            // creation!
            bool sqlError = false;
            if (tablesExist() == false) {
                MessageBox.Show("The tables have already been dropped.");
            } else {
                SqlCommand cmd = con.CreateCommand();
                try {
                    con.Open();
                    cmd.CommandText = "drop table Character";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "drop table Show";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "drop table ProdCo";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "drop table Actor";
                    cmd.ExecuteNonQuery();

                } catch (Exception ex) {
                    MessageBox.Show("Error dropping tables: " + ex.Message);
                    sqlError = true;
                } finally {
                    con.Close();
                }

                if (sqlError == false) {
                    MessageBox.Show("Tables dropped.");
                }
                disableRecordTabs();
           
            }
        }

        private void buttonDisplayRecords_Click(object sender, EventArgs e) {
            // Stash all this stuff into textBoxDisplayRecs.Text

            // Dunno how we're supposed to present these records, so I'm just gonna dump each table
            // into the box, using newlines to separate records and such.
            textBoxDisplayRecs.Text = null;
            SqlCommand cmd = con.CreateCommand();

            textBoxDisplayRecs.AppendText("CHARACTER\r\n");
            textBoxDisplayRecs.AppendText("CharID\t\tFName\t\tLName\t\tShowID\t\tActorID\r\n");
            try {
                // yank all Character records
                con.Open();
                cmd.CommandText = 
                    "Select CAST(CharID AS CHAR(4)) AS CharID, FName, LName, " +
                    "CAST(ShowID AS CHAR(4)) AS ShowID, " +
                    "CAST(ActorID AS CHAR(4)) AS ActordID " +
                    "from Character ORDER BY LNAME";
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    textBoxDisplayRecs.AppendText(String.Format("{0}\t\t{1}{2}\t{3}\t\t{4}\r\n",
                        reader.GetString(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.GetString(4)));
                }
            } catch (Exception ex) {
                MessageBox.Show("Error reading from Character table: " + ex.Message);
            }
            finally {
                con.Close();
            }


            textBoxDisplayRecs.AppendText("\r\nACTOR\r\n");
            textBoxDisplayRecs.AppendText("ActorID\t\tFName\t\t\tLName\r\n");
            try {
                // yank all Actor records
                con.Open();
                cmd.CommandText = "select CAST(ActorID AS VARCHAR) AS ActorID, FName, LName from Actor order by LNAME";
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    textBoxDisplayRecs.AppendText(String.Format("{0}\t\t{1}\t{2}\r\n",
                        reader.GetString(0), reader.GetString(1), reader.GetString(2)));
                }
            } catch (Exception ex) {
                MessageBox.Show("Error reading from Actor table: " + ex.Message);
            }
            finally {
                con.Close();
            }

            textBoxDisplayRecs.AppendText("\r\nSHOW\r\n");
            textBoxDisplayRecs.AppendText("ShowID\t\t\tName\t\t\tprodCoID\r\n");
            try {
                // yank all Show records
                con.Open();
                cmd.CommandText = "select CAST(ShowID AS VARCHAR) AS ShowID, " +
                    "Name, CAST(prodCoID AS VARCHAR) AS prodCoID from Show order by Name";
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    textBoxDisplayRecs.AppendText(String.Format("{0}\t\t\t{1}\t{2}\r\n",
                        reader.GetString(0), reader.GetString(1), reader.GetString(2)));
                }
            } catch (Exception ex) {
                MessageBox.Show("Error reading from Show table: " + ex.Message);
            }
            finally {
                con.Close();
            }

            textBoxDisplayRecs.AppendText("\r\nPRODCO\r\n");
            textBoxDisplayRecs.AppendText("prodCoID\t\t\tName\r\n");
            try {
                // yank all Show records
                con.Open();
                cmd.CommandText = "select Name, CAST(prodCoID AS VARCHAR) AS prodCoID from ProdCo order by Name";
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    textBoxDisplayRecs.AppendText(String.Format("{0}\t\t\t{1}\r\n",
                        reader.GetString(1), reader.GetString(0)));
                }
            } catch (Exception ex) {
                MessageBox.Show("Error reading from ProdCo table: " + ex.Message);
            }
            finally {
                con.Close();
            }

            // change button text--once records are shown in text box,
            // "refresh" seems a more appropriate verb than "display"
            buttonDisplayRecords.Text = "Refresh Listing";
        }// end buttonDisplayRecords_Click

        private void buttonInsertActor_Click(object sender, EventArgs e) {
            string fName = textBoxNewActorFName.Text.Replace("'", "''");
            string lName = textBoxNewActorLName.Text.Replace("'", "''");

            // Set up parameteriation stuffs here.
            string sqlCommand = "INSERT INTO Actor VALUES(@FName, @LName)";

            using (SqlCommand cmd = new SqlCommand(sqlCommand)) {
                cmd.Parameters.Add("@LName", System.Data.SqlDbType.NVarChar, 30);
                cmd.Parameters.Add("@FName", System.Data.SqlDbType.NVarChar, 100);

                cmd.Parameters["@FName"].Value = fName;
                cmd.Parameters["@LName"].Value = lName;
                cmd.Connection = con;

                try {
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                } catch (Exception ex) {
                    MessageBox.Show("Error inserting into Actor table: " + ex.Message);
                } finally {
                    con.Close();
                }
            }

            // clean textboxen!
            textBoxNewActorFName.Text = "";
            textBoxNewActorLName.Text = "";
            // update the index dictionaries/comboboxen
            updateIndices();

            
        }
        private void buttonInsertProdCo_Click(object sender, EventArgs e) {
            string Name = textBoxNewProdCoName.Text.Replace("'", "''");

            string sqlCommand = "INSERT INTO ProdCo VALUES(@Name)";

            using (SqlCommand cmd = new SqlCommand(sqlCommand)) {
                cmd.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar, 30);
                cmd.Parameters["@Name"].Value = Name;
                cmd.Connection = con;

                try {
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                } catch (Exception ex) {
                    MessageBox.Show("Error inserting into ProdCo table: " + ex.Message);
                } finally {
                    con.Close();
                }
            }
            // clear textbox for next run
            textBoxNewProdCoName.Text = "";
            // Presumably we have a new production company, so update
            // the index dictionary/combobox.
            updateIndices();

        }
        private void buttonInsertShow_Click(object sender, EventArgs e) {
            // No null IDs allowed!
            if (textBoxNewShowProdCoID.Text.Length == 0) {
                string errMsg = "Production Company ID cannot be null. " +
                    "You can use the \"Display Records\" function to " +
                    "view a list of production companies and their IDs.";
                MessageBox.Show(errMsg);
            } else  {
                // Set up parameterization stuffs here.
                string name = textBoxNewShowName.Text.Replace("'", "''");

                string sCommand = "INSERT INTO Show VALUES(@Name, @ProdCoID)";
                using (SqlCommand cmd = new SqlCommand(sCommand)) {
                    cmd.Parameters.Add("@Name", System.Data.SqlDbType.Char, 30);
                    cmd.Parameters.Add("@ProdCoID", System.Data.SqlDbType.Int);

                    cmd.Parameters["@Name"].Value = name;
                    cmd.Parameters["@ProdCoID"].Value =
                        Convert.ToInt32(textBoxNewShowProdCoID.Text);
                    cmd.Connection = con;

                    try {
                        cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                    } catch (Exception ex) {
                        MessageBox.Show("Error inserting into Show table: " + ex.Message);
                    } finally {
                        con.Close();
                    }
                }
            }
            // clean textboxen for next run
            textBoxNewShowName.Text = "";
            textBoxNewShowProdCoID.Text = "";
            // update indices
            updateIndices();
        }
        private void buttonInsertCharacter_Click(object sender, EventArgs e) {
            // No null IDs!
            if (textBoxNewCharActorID.Text.Length == 0 | textBoxNewCharShowID.Text.Length == 0) {
                MessageBox.Show("You must supply both an Actor ID and a Show ID. " +
                    "You can use the \"Display Records\" function " +
                    "to find actor and show IDs.");
            } else {
                // Set up parameterization stuffs here.
                string fName = textBoxNewCharFName.Text.Replace("'", "''");
                string lName = textBoxNewCharLName.Text.Replace("'", "''");


                string sCommand = "INSERT INTO Character VALUES(" +
                    "@fName, @lName, @ShowID, @ActorID)";
                using (SqlCommand cmd = new SqlCommand(sCommand)) {
                    cmd.Parameters.Add("@fName", System.Data.SqlDbType.Char, 30);
                    cmd.Parameters.Add("@lName", System.Data.SqlDbType.Char, 20);
                    cmd.Parameters.Add("@ShowID", System.Data.SqlDbType.Int);
                    cmd.Parameters.Add("@ActorID", System.Data.SqlDbType.Int);

                    cmd.Parameters["@fName"].Value = fName;
                    cmd.Parameters["@lName"].Value = lName;
                    cmd.Parameters["@ShowID"].Value = Convert.ToInt32(textBoxNewCharShowID.Text);
                    cmd.Parameters["@ActorID"].Value = Convert.ToInt32(textBoxNewCharActorID.Text);

                    cmd.Connection = con;

                    try {
                        cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                    } catch (Exception ex) {
                        MessageBox.Show("Error inserting into Character table: " + ex.Message);
                    } finally {
                        con.Close();
                    }
                }
            }
            // clear textboxen for next run.
            textBoxNewCharActorID.Text = null;
            textBoxNewCharShowID.Text = null;
            textBoxNewCharFName.Text = null;
            textBoxNewCharLName.Text = null;

            // DB has changed, so update index dictionaries/comboboxen.
            updateIndices(); 
        }
     
        private void buttonDeleteActor_Click(object sender, EventArgs e) {
            string actorID = ComboBoxDeleteActor.SelectedValue.ToString();

            string sCommand = "DELETE FROM Actor WHERE ActorID=@ActorID";
            using (SqlCommand cmd = new SqlCommand(sCommand)) {
                cmd.Parameters.Add("@ActorID", System.Data.SqlDbType.Int);
                cmd.Parameters["@ActorID"].Value = Convert.ToInt32(actorID);

                cmd.Connection = con;

                try {
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                } catch (Exception ex) {
                    MessageBox.Show("Error deleting from Actor table: " + ex.Message);
                } finally {
                    con.Close();
                }
            }

            //Clear textbox for next run
            ComboBoxDeleteActor.Text = null;
            // Update indices
            updateIndices();
        }
        private void buttonDeleteProductionCo_Click(object sender, EventArgs e) {
            string prodCoID = comboBoxDeleteProdCo.SelectedValue.ToString();
            string sCommand = "DELETE FROM ProdCo WHERE ProdCoID=@ProdCoID";
            using(SqlCommand cmd = new SqlCommand(sCommand)) {
                cmd.Parameters.Add("@ProdCoID", System.Data.SqlDbType.Int);
                cmd.Parameters["@ProdCoID"].Value = Convert.ToInt32(prodCoID);

                cmd.Connection = con;

                try {
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                } catch (Exception ex) {
                    MessageBox.Show("Error deleting from ProdCo table: " + ex.Message);
                } finally {
                    con.Close();
                }
            }

            // Clear textbox for next run
            comboBoxDeleteProdCo.Text = null;
            // Update indices b/c DB has changed
            updateIndices();
        }// end buttonDeleteProductionCo_Click

        private void buttonDeleteShow_Click(object sender, EventArgs e) {
            string showID = comboBoxDeleteShow.SelectedValue.ToString();
            string sCommand = "DELETE FROM Show WHERE ShowID=@ShowID";

            using (SqlCommand cmd = new SqlCommand(sCommand)) {
                cmd.Parameters.Add("@ShowID", System.Data.SqlDbType.Int);
                cmd.Parameters["@ShowID"].Value = Convert.ToInt32(showID);

                cmd.Connection = con;

                try {
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                } catch (Exception ex) {
                    MessageBox.Show("Error deleting from Show table: " + ex.Message);
                }
                finally {
                    con.Close();
                }
            }
            // Clear textbox for next run
            comboBoxDeleteShow.Text = null;
            // Update indices
            updateIndices();

        }


        private void buttonDeleteCharacter_Click(object sender, EventArgs e) {
            string charID = comboBoxDeleteCharacter.SelectedValue.ToString();
            string sCommand = "DELETE FROM Character WHERE CharID=@CharID";

            using (SqlCommand cmd = new SqlCommand(sCommand)) {
                cmd.Parameters.Add("@CharID", System.Data.SqlDbType.Int);
                cmd.Parameters["@CharID"].Value = Convert.ToInt32(charID);

                cmd.Connection = con;

                try {
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                } catch (Exception ex) {
                    MessageBox.Show("Error deleting from Character table: " + ex.Message);
                } finally {
                    con.Close();
                }
            }
            // Clear textbox for next run
            comboBoxDeleteCharacter.Text = null;
            // Update indices
            updateIndices();

        }

        private void buttonModActor_Click(object sender, EventArgs e) {
            string actorID = comboBoxModActor.SelectedValue.ToString();
            string fName = textBoxModActorFName.Text.Replace("'", "''");
            string lName = textBoxModActorLName.Text.Replace("'", "''");

            string sCommand = "UPDATE Actor SET LName=@LName, FName=@FName WHERE ActorID=@ActorID";

            if (textBoxModActorFName.Text.Length == 0 |
                textBoxModActorLName.Text.Length == 0) {
                string errMsg = "Please don't leave any fields blank!";
                MessageBox.Show(errMsg);
            } else {

                using (SqlCommand cmd = new SqlCommand(sCommand)) {
                    cmd.Parameters.Add("@FName", System.Data.SqlDbType.NVarChar, 30);
                    cmd.Parameters.Add("@LName", System.Data.SqlDbType.NVarChar, 100);
                    cmd.Parameters.Add("@ActorID", System.Data.SqlDbType.Int);

                    cmd.Parameters["@FName"].Value = fName;
                    cmd.Parameters["@LName"].Value = lName;
                    cmd.Parameters["@ActorID"].Value = Convert.ToInt32(actorID);

                    cmd.Connection = con;

                    try {
                        cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                    } catch (Exception ex) {
                        MessageBox.Show("Error modifying actor record: " + ex.Message);
                    } finally {
                        con.Close();
                    }
                }
                // Clear text box for next time, and update indices to reflect change.
                textBoxModActorFName.Text = null;
                textBoxModActorLName.Text = null;
                updateIndices();
            }


        }
        private void buttonModifyProdCo_Click(object sender, EventArgs e) {
            string prodCoID = comboBoxModProdCo.SelectedValue.ToString();
            string name = textBoxModProdCoName.Text.Replace("'", "''");
            string sCommand = "UPDATE ProdCo SET Name = @Name WHERE prodCoID = @ProdCoID";

            if (textBoxModProdCoName.Text.Length == 0) {
                MessageBox.Show("Name cannot be blank!");
            } else {
                using (SqlCommand cmd = new SqlCommand(sCommand)) {
                    cmd.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar, 30);
                    cmd.Parameters.Add("@ProdCoID", System.Data.SqlDbType.Int);

                    cmd.Parameters["@Name"].Value = name;
                    cmd.Parameters["@ProdCoID"].Value = Convert.ToInt32(prodCoID);

                    cmd.Connection = con;

                    try {
                        cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                    } catch (Exception ex) {
                        MessageBox.Show("Error modifying prod. co. record: " + ex.Message);
                    } finally {
                        con.Close();
                    }
                }
                // Cleanup and what-not
                textBoxModProdCoName.Text = null;
                updateIndices();

            }// end if

        }
        private void buttonModifyShow_Click(object sender, EventArgs e) {
            string showID = comboBoxModShow.SelectedValue.ToString();
            string name = textBoxModShowName.Text.Replace("'", "''");
            string sCommand = "UPDATE Show SET Name=@Name WHERE ShowID=@ShowID";

            if (textBoxModShowName.Text.Length == 0 | textBoxModShowProdCoID.Text.Length == 0) {
                MessageBox.Show("Please don't leave any fields blank!");
            } else {
                using (SqlCommand cmd = new SqlCommand(sCommand)) {
                    cmd.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar, 30);
                    cmd.Parameters.Add("@ShowID", System.Data.SqlDbType.Int);

                    cmd.Parameters["@Name"].Value = name;
                    cmd.Parameters["@ShowID"].Value = Convert.ToInt32(showID);

                    cmd.Connection = con;

                    try {
                        cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                    } catch (Exception ex) {
                        MessageBox.Show("Error modifying show record: " + ex.Message);
                    } finally {
                        con.Close();
                    }
                }
                // Cleanup and what-not
                textBoxModShowProdCoID.Text = null;
                textBoxModShowName.Text = null;
                updateIndices();
            }
        }
        private void buttonModCharacter_Click(object sender, EventArgs e) {
            string charID = comboBoxModCharacter.SelectedValue.ToString();
            string fName = textBoxModCharFName.Text.Replace("'", "''");
            string lName = textBoxModCharLName.Text.Replace("'", "''");
            string actorID = textBoxModCharActorID.Text.ToString();
            string showID = textBoxModCharShowID.Text.ToString();

            string sCommand = "UPDATE Character SET FName = @FName, LName = @LName, ShowID=@ShowID, ActorID=@ActorID WHERE CharID=@CharID";

            if (textBoxModCharActorID.Text.Length == 0 |
                textBoxModCharShowID.Text.Length == 0 |
                textBoxModCharFName.Text.Length == 0 |
                textBoxModCharLName.Text.Length == 0) {
                MessageBox.Show("Please don't leave any fields blank!");
            } else {
                using (SqlCommand cmd = new SqlCommand(sCommand)) {
                    cmd.Parameters.Add("@FName", System.Data.SqlDbType.NVarChar, 30);
                    cmd.Parameters.Add("@LName", System.Data.SqlDbType.NVarChar, 100);
                    cmd.Parameters.Add("@ShowID", System.Data.SqlDbType.Int);
                    cmd.Parameters.Add("@ActorID", System.Data.SqlDbType.Int);
                    cmd.Parameters.Add("@CharID", System.Data.SqlDbType.Int);

                    cmd.Parameters["@FName"].Value = fName;
                    cmd.Parameters["@LName"].Value = lName;
                    cmd.Parameters["@ShowID"].Value = Convert.ToInt32(showID);
                    cmd.Parameters["@ActorID"].Value = Convert.ToInt32(actorID);
                    cmd.Parameters["@CharID"].Value = Convert.ToInt32(charID);

                    cmd.Connection = con;

                    try {
                        cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                    } catch (Exception ex) {
                        MessageBox.Show("Error modifying character record: " + ex.Message);
                    } finally {
                        con.Close();
                    }
                }
                // Prep for next run
                textBoxModCharFName.Text = null;
                textBoxModCharLName.Text = null;
                textBoxModCharShowID.Text = null;
                textBoxModCharActorID.Text = null;
                updateIndices();
            }
        }

        private void updateIndices() {
            SqlCommand cmd = con.CreateCommand();

            // PROBLEM: the dictionaries must be cleared out each time
            // this runs. otherwise we get exceptions due to already-existing
            // members.

            // Load production company data.
            prodCoIndex.Clear();
            try {
                con.Open();
                cmd.CommandText = "SELECT CAST(prodCoID AS CHAR(4)) AS prodCoID, Name " +
                                  "FROM ProdCo";
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read()) {
                    string name = (string)reader["Name"];
                    string id = (string)reader["prodCoID"];
                    prodCoIndex.Add(name.Trim(), id.Trim());

                }
            } catch (Exception ex) {
                MessageBox.Show("Error selecting from ProdCo table: " + ex.Message);
            } finally {
                con.Close();
            }

            // stuff the comboboxen with key-value data from the prodco dictionary
            comboBoxDeleteProdCo.DataSource = new BindingSource(prodCoIndex, null);
            comboBoxDeleteProdCo.DisplayMember = "Key";
            comboBoxDeleteProdCo.ValueMember = "Value";

            comboBoxModProdCo.DataSource = new BindingSource(prodCoIndex, null);
            comboBoxModProdCo.DisplayMember = "Key";
            comboBoxModProdCo.ValueMember = "Value";


            // Load actor data
            actorIndex.Clear();
            try {
                con.Open();
                cmd.CommandText = "SELECT CAST(ActorID AS CHAR(4)) AS ActorID, " +
                    "RTRIM(FName) + ' ' + RTRIM(LName) AS Name FROM Actor;";
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    string name = (string)reader["Name"];
                    string id = (string)reader["ActorID"];
                    actorIndex.Add(name.Trim(), id.Trim());
                }
            } catch (Exception ex) {
                MessageBox.Show("Error selecting from Actor table: " + ex.Message);
            } finally {
                con.Close();
            }

            // Load combobox!
            ComboBoxDeleteActor.DataSource = new BindingSource(actorIndex, null);
            ComboBoxDeleteActor.DisplayMember = "Key";
            ComboBoxDeleteActor.ValueMember = "Value";

            comboBoxModActor.DataSource = new BindingSource(actorIndex, null);
            comboBoxModActor.DisplayMember = "Key";
            comboBoxModActor.ValueMember = "Value";

            // Load show data
            showIndex.Clear();
            try {
                con.Open();
                cmd.CommandText = "SELECT CAST(ShowID AS CHAR(4)) AS ShowID, Name FROM Show";
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    string name = (string)reader["Name"];
                    string id = (string)reader["ShowID"];
                    showIndex.Add(name.Trim(), id.Trim());
                }
            } catch (Exception ex) {
                MessageBox.Show("Error selecting from Show table: " + ex.Message);
            } finally {
                con.Close();
            }

            // Load combobox
            comboBoxDeleteShow.DataSource = new BindingSource(showIndex, null);
            comboBoxDeleteShow.DisplayMember = "Key";
            comboBoxDeleteShow.ValueMember = "Value";

            comboBoxModShow.DataSource = new BindingSource(showIndex, null);
            comboBoxModShow.DisplayMember = "Key";
            comboBoxModShow.ValueMember = "Value";

            // Character data
            charIndex.Clear();
            try {
                con.Open();
                cmd.CommandText = "SELECT CAST(CharID AS CHAR(4)) AS CharID, RTRIM(FName) + " +
                    "' ' + RTRIM(LName) AS Name FROM Character";
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    string name = (string)reader["Name"];
                    string id = (string)reader["CharID"];
                    charIndex.Add(name.Trim(), id.Trim());
                }
            } catch (Exception ex) {
                MessageBox.Show("Error selecting from Character table: " + ex.Message);
            } finally {
                con.Close();
            }
            comboBoxDeleteCharacter.DataSource = new BindingSource(charIndex, null);
            comboBoxDeleteCharacter.DisplayMember = "Key";
            comboBoxDeleteCharacter.ValueMember = "Value";

            comboBoxModCharacter.DataSource = new BindingSource(charIndex, null);
            comboBoxModCharacter.DisplayMember = "Key";
            comboBoxModCharacter.ValueMember = "Value";
        }

        private void disableRecordTabs() {
            // Disable the tabls that allow for interacting with records.
            tabPageDisplayRecs.Enabled = false;
            tabPageInsertRecs.Enabled = false;
            tabPageDeleteRecs.Enabled = false;
            tabPageModifyRecs.Enabled = false;
        }
        private void enableRecordTabs() {
            // Enable the tabs that allow for interacting with records.
            tabPageDisplayRecs.Enabled = true;
            tabPageInsertRecs.Enabled = true;
            tabPageDeleteRecs.Enabled = true;
            tabPageModifyRecs.Enabled = true;
        }

    }// end partial class Form1

}// end namesapce