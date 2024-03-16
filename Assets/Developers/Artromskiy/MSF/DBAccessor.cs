using Amazon.DynamoDBv2;
using Amazon;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System.Collections.Generic;

namespace DB
{
    public static class DBAccessor
    {
        private static DynamoDBContext _context;
        private static AmazonDynamoDBClient _client;

        static DBAccessor()
        {
            Update();
        }

        /// <summary>
        /// Для неявного вызова конструктора
        /// </summary>
        public static void Initialize()
        {

        }

        /// <summary>
        /// Вызывать, если требуется обновить информацию таблиц
        /// </summary>
        public static void Update()
        {
            LoadCharactersTable();
            LoadItemsTable();
        }
        
        public static AmazonDynamoDBClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new AmazonDynamoDBClient
                    (
                        "AKIARG5ZJB7XK7IZRBSE",
                        "kHlU/sLvUYy6pmlloFtPX46LG6TjTFddZ1/MBsBX",
                        RegionEndpoint.GetBySystemName(RegionEndpoint.EUCentral1.SystemName)
                    );
                }
                return _client;
            }
        }
        public static DynamoDBContext Context
        {
            get
            {
                if (_context == null)
                {
                    _context = new DynamoDBContext(Client);
                }
                return _context;
            }
        }

        public static List<Document> Items { get; private set; }

        public static List<Document> Characters { get; private set; }


        private static void LoadItemsTable()
        {
            Table.LoadTableAsync(Client, "Items", (res) =>
             {
                 if (res.Exception == null)
                 {
                     ScanFilter scanFilter = new ScanFilter();
                     Search search = res.Result.Scan(scanFilter);
                     List<Document> documentList = new List<Document>();

                     search.GetNextSetAsync((listRes) =>
                     {
                         if (listRes.Exception == null)
                         {
                             UnityEngine.Debug.Log("Items downloaded");
                             documentList.AddRange(listRes.Result);
                             Items = documentList;
                         }
                         else
                         {
                             throw listRes.Exception;
                         }
                     });
                 }
                 else
                 {
                     throw res.Exception;
                 }
             });
        }

        private static void LoadCharactersTable()
        {
            Table.LoadTableAsync(Client, "Characters", (res) =>
            {
                if (res.Exception == null)
                {
                    ScanFilter scanFilter = new ScanFilter();
                    Search search = res.Result.Scan(scanFilter);
                    List<Document> documentList = new List<Document>();

                    search.GetNextSetAsync((listRes) =>
                    {
                        if (listRes.Exception == null)
                        {
                            UnityEngine.Debug.Log("Characters downloaded");
                            documentList.AddRange(listRes.Result);
                            Characters = documentList;
                        }
                        else
                        {
                            throw listRes.Exception;
                        }
                    });
                }
                else
                {
                    throw res.Exception;
                }
            });
        }
    }
}
