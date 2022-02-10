using System;
using System.Collections.Generic;

namespace CarService
{
    class Program
    {
        static void Main(string[] args)
        {
            int clientsCount = 10;
            int detailId = 1;
            int cashBalance = 1000;

            List<Detail> carComponents = new List<Detail>
            {
                new(detailId++, "Колесо", 250),
                new(detailId++, "Фара", 500),
                new(detailId++, "Лобовое стекло", 1000),
                new(detailId++, "Бампер", 1500),
                new(detailId++, "Двигатель", 2500)
            };

            Queue<Client> clients = new Queue<Client>();

            for (int i = 0; i < clientsCount; i++)
            {
                clients.Enqueue(new Client(carComponents));
            }

            Service service = new Service(carComponents, clients, cashBalance);

            service.Work();
        }
    }

    class Service
    {
        private Warehouse _warehouse;
        private Queue<Client> _clients;
        private int _cashBalance;
        private int _penalty;

        public Service(List<Detail> carComponents, Queue<Client> clients, int cashBalance)
        {
            _warehouse = new Warehouse(carComponents);
            _clients = clients;
            _cashBalance = cashBalance;
            _penalty = 250;
        }

        public void Work()
        {
            bool isWork = true;

            while (isWork)
            {
                Console.Clear();
                ShowInfo();
                ServeClient();

                if (_clients.Count == 0)
                {
                    isWork = false;
                }
            }

            Console.Clear();
            Console.WriteLine($"Вы закончили сегодняшнее обслуживание.\nПо итогом дня Ваш баланс составил: {_cashBalance}$");
            Console.ReadKey();
        }

        private void ShowInfo()
        {
            Console.WriteLine($"Баланс Авто-сервиса \"Авто-Сервис\": {_cashBalance}$\n");
            Console.WriteLine($"Нажмите клавишу, чтобы облсужить клиента (клиентов в очереди({_clients.Count}))\n");
            Console.ReadKey();
        }

        private void ServeClient()
        {
            int finalPrice = 0;
            Client client = _clients.Peek();
            Detail brokenDetail = client.GetBrokenDetail();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Клиенту необходима замена детали - {brokenDetail.Title}\n");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("Список деталей на складе:");
            _warehouse.ShowDetailsList();
            Console.Write("\nВыберите нужную деталь на складе и введите ID: ");
            string userInput = Console.ReadLine();
            bool success = int.TryParse(userInput, out int inputId);

            if (success)
            {
                bool isFind = _warehouse.TryFindDetail(inputId, out Detail necessaryDetail);


                if (isFind)
                {
                    bool isRightDetail = brokenDetail.Id == necessaryDetail.Id;

                    if (isRightDetail)
                    {
                        _warehouse.UseDetail(necessaryDetail);
                        FinishClientServe("Клиент доволен.\n");
                        finalPrice += CalculateWorkCost(necessaryDetail);
                    }
                    else
                    {
                        _warehouse.UseDetail(necessaryDetail);
                        FinishClientServe("Клиент разочарован. Вы заменили ему не ту деталь и должны возместить ущерб\n");
                        finalPrice -= CalculateWorkCost(necessaryDetail) + _penalty;
                    }
                }
                else
                {
                    FinishClientServe("У Вас отсутствует такая деталь, придётся выплатить неустойку\n");
                    finalPrice -= _penalty;
                }
            }
            else
            {
                Console.WriteLine("\nНекорректный ввод. Введите id детали");
            }

            Console.WriteLine($"Итоговый счёт: {finalPrice}$");
            _cashBalance += finalPrice;
            Console.ReadKey();
        }

        private int CalculateWorkCost(Detail detail)
        {
            int totalPercent = 100;
            int percentShare = 30;
            int workCost = detail.Cost / totalPercent * percentShare;
            int finalPrice = detail.Cost + workCost;

            return finalPrice;
        }

        private void FinishClientServe(string message)
        {
            Console.WriteLine(message);
            _clients.Dequeue();
        }
    }

    class Warehouse
    {
        private List<Detail> _carComponents;
        private List<Detail> _details = new List<Detail>();

        public Warehouse(List<Detail> carComponents)
        {
            _carComponents = carComponents;
            CreateDetailsStock();
        }

        public void ShowDetailsList()
        {
            foreach (Detail detail in _carComponents)
            {
                detail.ShowInfo();
                Console.WriteLine($" - в наличии {CalculateDetailsQuantity(detail.Id)} шт.");
            }
        }

        public bool TryFindDetail(int id, out Detail foundDetail)
        {
            foundDetail = null;
            bool isFind = false;

            foreach (Detail detail in _details)
            {
                if (id == detail.Id)
                {
                    foundDetail = detail;
                    isFind = true;
                    break;
                }
            }

            return isFind;
        }

        public void UseDetail(Detail detail)
        {
            _details.Remove(detail);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nДеталь была заменена\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public List<Detail> CreateDetailsStock()
        {
            Random random = new Random();
            int minDetailsCount = 1;
            int maxDetailsCount = 3;

            for (int i = 0; i < _carComponents.Count; i++)
            {
                int randomDetailsCount = random.Next(minDetailsCount, maxDetailsCount + 1);
                for (int j = 0; j < randomDetailsCount; j++)
                {
                    _details.Add((Detail)_carComponents[i].Clone());
                }
            }

            return _details;
        }

        public int CalculateDetailsQuantity(int id)
        {
            int quantity = 0;

            foreach (Detail detail in _details)
            {
                if (detail.Id == id)
                {
                    quantity++;
                }
            }

            return quantity;
        }
    }

    class Client
    {
        private List<Detail> _carComponents;

        public Client(List<Detail> carComponents)
        {
            _carComponents = carComponents;
        }

        public Detail GetBrokenDetail()
        {
            Random random = new Random();
            int randomDetail = random.Next(0, _carComponents.Count);
            return _carComponents[randomDetail];
        }
    }

    class Detail : ICloneable

    {
        private int _id;
        private string _title;
        private int _cost;

        public Detail(int id, string title, int cost)
        {
            _id = id;
            _title = title;
            _cost = cost;
        }

        public int Id => _id;

        public string Title => _title;

        public int Cost => _cost;

        public object Clone()
        {
            return new Detail(_id, _title, _cost);
        }

        public void ShowInfo()
        {
            Console.Write($"ID: {_id} - {_title} ({_cost}$)");
        }
    }

    public interface ICloneable
    {
        object Clone();
    }
}