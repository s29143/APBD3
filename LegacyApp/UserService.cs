using System;

namespace LegacyApp
{
    public class UserService
    {
        private IClientRepo _clientRepo { get; }
        private const int MinAge = 21;
        private const int CreditLimit = 500;

        public UserService()
        {
            _clientRepo = new ClientRepository();
        }
        
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            var user = new User
            {
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
            user.Client = _clientRepo.GetById(clientId);
            if(!ValidateUser(user) || !CheckCreditLimit(user))
            {
                return false;
            }
            UserDataAccess.AddUser(user);
            return true;
        }

        public bool ValidateUser(User user)
        {
            if (string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName))
            {
                return false;
            }

            if (!user.EmailAddress.Contains("@") && !user.EmailAddress.Contains("."))
            {
                return false;
            }

            var now = DateTime.Now;
            int age = now.Year - user.DateOfBirth.Year;
            if (now.Month < user.DateOfBirth.Month || (now.Month == user.DateOfBirth.Month && now.Day < user.DateOfBirth.Day))
                age--;

            if (age < MinAge)
            {
                return false;
            }

            return true;
        }

        public bool CheckCreditLimit(User user)
        {

            if (user.Client.Type != "VeryImportantClient")
            {
                using var userCreditService = new UserCreditService();
                int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                if (user.Client.Type == "ImportantClient")
                {
                    creditLimit = creditLimit * 2;
                }
                else
                {
                    user.HasCreditLimit = true;
                }
                user.CreditLimit = creditLimit;
                if (user.HasCreditLimit && user.CreditLimit < CreditLimit)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
