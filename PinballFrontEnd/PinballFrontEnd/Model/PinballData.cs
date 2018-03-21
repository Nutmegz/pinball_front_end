using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using NLog;

namespace PinballFrontEnd.Model
{
    //Class Stores all data that needs to be saved for single file saving
    public class PinballData
    {
        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //Data (Saved on SaveDatabase)
        public ObservableCollection<PinballSystem> SystemList { get; set; } = new ObservableCollection<PinballSystem>();
        public ObservableCollection<PinballTable> TableList { get; set; } = new ObservableCollection<PinballTable>();
        public MediaLocation MediaLocation { get; set; } = new MediaLocation();
        public Keybindings BIND { get; set; } = new Keybindings();

        //Default Constructor
        public PinballData()
        {
            //Populate Default Objects to avoid null exceptions
            SystemList = new ObservableCollection<PinballSystem>();
            TableList = new ObservableCollection<PinballTable>();
            MediaLocation = new MediaLocation();
            BIND = new Keybindings();
        }

        public PinballData(string databasepath)
        {
            if (!System.IO.File.Exists(databasepath))
                SaveDatabase(databasepath);
                            

            LoadDatabase(databasepath);


        }

        public void LoadDatabase(string databasepath)
        {
            logger.Info($"Loading Database {databasepath}");
            try
            {
                //Load if database exists
                if (System.IO.File.Exists(databasepath))
                {
                    var temp = JsonConvert.DeserializeObject<PinballData>(System.IO.File.ReadAllText(databasepath));

                    if (temp.BIND != null)
                    {
                        BIND = temp.BIND;
                    }

                    if (temp.SystemList != null)
                    {
                        SystemList = temp.SystemList;
                    }

                    if (temp.TableList != null)
                    {
                        TableList = temp.TableList;
                    }

                    if (temp.MediaLocation != null)
                    {
                        MediaLocation = temp.MediaLocation;
                    }
                }

                //Sort Database
                SortSystemsTables();
            }
            catch (Exception e)
            {
                logger.Error(e, "LoadDatabase Error");
                throw;
            }
        }

        public void SaveDatabase(string databasepath)
        {
            try
            {
                //Sort Systems and Tables
                SortSystemsTables();

                logger.Info($"Saving Database: {databasepath}");

                System.IO.File.WriteAllText(databasepath, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch (Exception e)
            {
                logger.Error(e, "SaveDatabase Error");
                throw;
            }


        }

        #region List Sorting

        //Sort the Table List by Description from A to Z
        public void SortTables()
        {
            logger.Trace("Sorting Tables By Description");
            try
            {
                TableList = new ObservableCollection<PinballTable>(TableList.OrderBy(i => i.Description));
            }
            catch (Exception e)
            {
                logger.Error(e, "SortTables Error");
                throw;
            }
        }

        //Sort the System List by Name from A to Z
        public void SortSystems()
        {
            logger.Trace("Sorting Systems By Name");
            try
            {
                SystemList = new ObservableCollection<PinballSystem>(SystemList.OrderBy(i => i.Name));
            }
            catch (Exception e)
            {
                logger.Error(e, "SortSytems Error");
                throw;
            }
        }

        //Combine both sort functions together
        public void SortSystemsTables()
        {
            SortTables();
            SortSystems();
        }

        #endregion



        //Find system that table goes with
        public PinballSystem FindSystem(PinballTable table)
        {
            return SystemList.Single(x => x.Name == table.System);
        }


        #region Table Select

        //Find the next table in the Table List
        public PinballTable NextTable(PinballTable table)
        {
            if (table != null)
            {
                var pinballtable = TableList.ElementAt(TableList.IndexOf(table) < TableList.Count() - 1 ? TableList.IndexOf(table) + 1 : 0);
                if (pinballtable.Enabled)
                {
                    return pinballtable;
                }
                else
                {
                    //Ignore Disabled Tables
                    return NextTable(pinballtable);
                }
            }
            else
            {
                return null;
            }

        }

        //Find the next table in the Table List + advance slots
        public PinballTable NextTable(PinballTable table, int advance)
        {
            //Console.WriteLine($"Next Table: {table.Description} -> {advance}");
            if (advance > 0)
            {
                return NextTable(NextTable(table), advance - 1);
            }

            return table;
        }


        //Find the previous table in the Table List
        public PinballTable PrevTable(PinballTable table)
        {
            if (table != null)
            {
                var pinballtable = TableList.ElementAt(TableList.IndexOf(table) > 0 ? TableList.IndexOf(table) - 1 : TableList.Count() - 1);
                if (pinballtable.Enabled)
                {
                    return pinballtable;
                }
                else
                {
                    //Ignore Disabled Tables
                    return PrevTable(pinballtable);
                }
            }
            else
            {
                return null;
            }

        }


        //Find the previous table in the Table List - advance slots
        public PinballTable PrevTable(PinballTable table, int advance)
        {
            //Console.WriteLine($"Prev Table: {table.Description} -> {advance}");
            if (advance > 0)
            {
                return PrevTable(PrevTable(table), advance - 1);
            }

            return table;
        }

        //Return a random table
        public PinballTable RandomTable()
        {
            //logger.Info($"Random Table");
            if (TableList.Count > 0)
            {
                //Select Random table
                Random rnd = new Random();
                return TableList.ElementAt(rnd.Next(0, TableList.Count));
            }
            else
            {
                return null;
            }


        }


        #endregion



    }
}
