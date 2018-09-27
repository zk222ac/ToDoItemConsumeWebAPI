using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;


namespace ToDoItemConsumeWebAPI
{
    class Program
    {
        static readonly HttpClient Client = new HttpClient();
        static readonly ToDoItem NewToDoItem = new ToDoItem();
        private static string _uri;
        static void Main(string[] args)
        {
            try
            {
                 RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }

        }

        static async Task RunAsync()
        {
            Console.WriteLine("Kindly enter your the Port no here:");
            string port = Console.ReadLine();
            Console.WriteLine("kindly Enter the Controller Name of hosted WebAPI");

            string controllerName = Console.ReadLine();
            string apiController = "api/"+controllerName;
            
            if(!string.IsNullOrEmpty(port))
            {
                // we need URI address of Services ( or called a resource)
                _uri = "https://localhost:"+port+"/" + apiController;
                Console.WriteLine($"URI:{_uri}");
            }
            else
            {
                _uri = "https://localhost:44321/api/ToDoItems";
                Console.WriteLine($"URI:{_uri}");
            }
            Client.BaseAddress = new Uri(_uri);
            // Remove all entries from request headers
            Client.DefaultRequestHeaders.Accept.Clear();
            // Request headers send only request in JSON format
            Client.DefaultRequestHeaders.Accept.Add( new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                
            Console.WriteLine("Enter Get. GetbyId - Post - Put - Delete - stop\r\n");
            string httpVerb = Console.ReadLine();
            while (!string.Equals(httpVerb, "stop", StringComparison.Ordinal))
            {
                if (httpVerb != null)
                {
                    IList<ToDoItem> toDoItems;
                    switch (httpVerb.ToUpper())
                    {
                        case "GET":
                            toDoItems = await GetToDoItemAsync();
                            ShowToDoItem(toDoItems);
                            break;
                        case "GETBYID":
                            Console.WriteLine("kindly insert specific Id for finding resource");
                            string id = Console.ReadLine();
                            var itemGetByIdResult = await GetToDoItemByIdAsync(id);
                            ShowToDoItemObj(itemGetByIdResult);
                            break;
                        case "POST":
                            Console.WriteLine("kindly insert resource for adding");
                            Console.WriteLine("Insert new Name..........");
                            NewToDoItem.Name = Console.ReadLine();
                            Console.WriteLine("Insert new IsCompleted..........");
                            NewToDoItem.IsComplete = Console.ReadLine();
                            var itemAddResult = await AddToDoItemAsync(NewToDoItem);
                            ShowToDoItemObj(itemAddResult);
                            break;
                        case "PUT":
                            Console.WriteLine("kindly edit the existing resource with specific Id");
                            Console.WriteLine("Insert specific Id for editing..........");
                            var updateId  = Console.ReadLine();
                            Console.WriteLine("call the existing resource with specific Id");
                            var updateNewByIdToDoItem = await GetToDoItemByIdAsync(updateId);
                            Console.WriteLine("Insert Name for editing..........");
                            updateNewByIdToDoItem.Name = Console.ReadLine();
                            Console.WriteLine("Insert IsCompleted for editing..........");
                            updateNewByIdToDoItem.IsComplete = Console.ReadLine();
                            UpdateToDoItemAsync(updateNewByIdToDoItem , updateId);
                            // after update we need to check the item with that specific id and show
                            var updateItemGetByIdResult = await GetToDoItemByIdAsync(updateId);
                            ShowToDoItemObj(updateItemGetByIdResult);
                            break;
                        case "DELETE":
                            Console.WriteLine("Insert specific Id for deleting..........");
                            var deleteId = Console.ReadLine();
                            DeleteToDoItemAsync(deleteId);
                            Thread.Sleep(2000);
                            // check this Id after deleting , it is existing still
                            toDoItems = await GetToDoItemAsync();
                            ShowToDoItem(toDoItems);
                            break;
                        default:
                            break;
                    }
                }
                Console.WriteLine("Enter Get. GetbyId - Post - Put - Delete - stop");
                httpVerb = Console.ReadLine();
            }
            Console.WriteLine("Program ends! Bye Bye... Press Enter to end the program");
            Console.ReadLine();
        }
        

        // Display the all list items of ToDoItem
         static void ShowToDoItem(IList<ToDoItem> toDoItem)
        {
            foreach (var item in toDoItem)
            {
                Console.WriteLine($"ID:{item.Id}--->Name:{item.Name}---->IsComplete:{item.IsComplete}");
            }
            Console.WriteLine(".............................................");
        }
        static void ShowToDoItemObj(ToDoItem item)
        {
                Console.WriteLine($"ID:{item.Id}--->Name:{item.Name}---->IsComplete:{item.IsComplete}\r\n");
        }

        // Send a GET request to retrieve a resource

        static async Task<IList<ToDoItem>> GetToDoItemAsync()
        {
            string content = await Client.GetStringAsync(_uri);
            IList<ToDoItem> tdiList = JsonConvert.DeserializeObject<IList<ToDoItem>>(content);
            return tdiList;
        }
        static async Task<ToDoItem> GetToDoItemByIdAsync(string id)
        {
            string uriId = _uri + "/" + id;
            HttpResponseMessage response = await Client.GetAsync(uriId);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception("Customer not found. Try another id");
            }
            response.EnsureSuccessStatusCode();
            string str = await response.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<ToDoItem>(str);
            return items;
        }

        static async Task<ToDoItem> AddToDoItemAsync(ToDoItem newToDoItem)
        {
            var jsonString = JsonConvert.SerializeObject(newToDoItem);
            Console.WriteLine("JSON: " + jsonString);
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await Client.PostAsync(_uri, content);
            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new Exception("Customer already exists. Try another id");
            }
            response.EnsureSuccessStatusCode();
            string str = await response.Content.ReadAsStringAsync();
            var newlyCreatedToDoItem = JsonConvert.DeserializeObject<ToDoItem>(str);
            return newlyCreatedToDoItem;
        }
        static async void UpdateToDoItemAsync(ToDoItem updateNewToDoItem , string id )
        {
            string uriId = _uri + "/" + id;
            var jsonString = JsonConvert.SerializeObject(updateNewToDoItem);
            Console.WriteLine("JSON: " + jsonString);
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await Client.PutAsync(uriId, content);
            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new Exception("Customer already exists. Try another id");
            }
             response.EnsureSuccessStatusCode();
        }
        static async void DeleteToDoItemAsync(string id)
        {
            string uriId = _uri + "/" + id;
            HttpResponseMessage response = await Client.DeleteAsync(uriId);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception("Customer not found with that Id. Try another id");
            }
            response.EnsureSuccessStatusCode();
        }
    }
}
