using SanuApi.Application.DTOs.Customer;
using SanuApi.Application.DTOs.Goal;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;

using System.Data;

namespace SanuApi.Aplication.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IHealthCustomerRepository _healthCustomerRepository;
        private readonly IGoalRepository _goalRepository;
        private readonly ICustomerMembershipRepository _customerMembershipRepository;
        private readonly IDbConnection _db;
        public CustomerService(ICustomerRepository customerRepository, IHealthCustomerRepository healthCustomerRepository, IGoalRepository goalRepository, ICustomerMembershipRepository customerMembershipRepository, IDbConnection db)
        {
            _customerRepository = customerRepository;
            _healthCustomerRepository = healthCustomerRepository;
            _goalRepository = goalRepository;
            _customerMembershipRepository = customerMembershipRepository;
            _db = db;
        }
        public async Task<bool> AddClassesAsync(int customerId, AddCustomerClassRequestDto classCustomer)
        {
            if (classCustomer.ClassIds == null || !classCustomer.ClassIds.Any())
                return false;

            foreach (var classId in classCustomer.ClassIds)
            {
                var id = await _customerRepository.AddClassesAsync(
                    new ClassCustomer
                    {
                        customerid = customerId,
                        classid = classId
                    });

                if (id <= 0)
                    return false;
            }

            return true;
        }

        public async Task<bool> AddMembershipAsync(int customerId, AddCustomerMembershipRequestDto membershipCustomer)
        {
            if (membershipCustomer.MembershipIds == null || !membershipCustomer.MembershipIds.Any())
                return false;

            
                foreach (var item in membershipCustomer.MembershipIds)
                {
                    var idMembership = await _customerMembershipRepository.AddAsync(new CustomerMembership
                    {
                        customerid = customerId,
                        membershipid = item.IdMembership,
                        startdate = item.StartDate,
                        enddate = item.EndDate
                    });
                    if (idMembership <= 0) throw new InvalidOperationException("No se pudo guardar la membresía del cliente.");
                }            

            return true;
        }


        public async Task<int> AddAsync(CustomerAddRequestDto customer)
        {
            int idCustomer = 0;
            if (_db.State != ConnectionState.Open)
                _db.Open();
            using var transaction = _db.BeginTransaction();
            try
            {
                var newCustomer = new Customer
                {
                    customername = customer.CustomerName,
                    customerlastname = customer.CustomerLastName,
                    datebirth = customer.DateBirth,
                    dni = customer.Dni,
                    celphone = customer.Celphone,
                    address = customer.Address,
                    ismale = customer.IsMale,
                    fechabaja = null,
                    fechaalta = DateTime.Now,
                    comentaries = customer.Comentaries
                };
                idCustomer = await _customerRepository.AddAsync(newCustomer);
                if (idCustomer <= 0) throw new InvalidOperationException("No se pudo guardar el cliente.");
                if (customer.Health != null)
                {
                    var healthCustomer = new HealthCustomer
                    {
                        customerid = idCustomer,
                        weight = customer.Health.Weight,
                        heigth = customer.Health.Height,
                        alergics = customer.Health.Alergics,
                        medicalCondicion = customer.Health.MedicalCondicion
                    };
                    var idHealth = await _healthCustomerRepository.AddAsync(healthCustomer);
                    if (idHealth <= 0) throw new InvalidOperationException("No se pudo guardar la condición médica del cliente.");

                }
                if (customer.idGoal.Any())
                {
                    foreach (var id in customer.idGoal)
                    {
                        var customerGoal = new CustomerGoal
                        {
                            customerid = idCustomer,
                            goalid = id
                        };
                        var idCustomerGoal = await _goalRepository.AddCustomerGoalAsync(customerGoal);
                        if (idCustomerGoal <= 0) throw new InvalidOperationException("No se pudo guardar el objetivo del cliente.");
                    }
                }

                if (customer.Memberships.Any())
                {

                    foreach (var item in customer.Memberships)
                    {
                        var idMembership = await _customerMembershipRepository.AddAsync(new CustomerMembership
                        {
                            customerid = idCustomer,
                            membershipid = item.IdMembership,
                            startdate = item.StartDate,
                            enddate = item.EndDate
                        });
                        if (idMembership <= 0) throw new InvalidOperationException("No se pudo guardar la membresía del cliente.");
                    }
                }
                if (customer.CustomerClasses.Any())
                {

                    foreach (var item in customer.CustomerClasses)
                    {
                        var idClassCustomer = await _customerRepository.AddClassesAsync(new ClassCustomer { classid = item, customerid = idCustomer });
                        if (idClassCustomer <= 0) throw new InvalidOperationException("No se pudo guardar la clase del cliente.");
                    }

                }
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }

            return idCustomer;
        }

        public Task<bool> DeleteAsync(int id)
        {
            var customer = _customerRepository.FindByIdAsync(id);
            if (customer == null) return Task.FromResult(false);

            return _customerRepository.DeleteAsync(id);
        }

        public async Task<CutsomerFindByIdResponseDto?> FindByIdAsync(int id)
        {
            var c = await _customerRepository.FindByIdAsync(id);
            if (c == null)
                return null;

            return new CutsomerFindByIdResponseDto
            {
                IdCustomer = c.id,
                CustomerName = c.customername,
                CustomerLastName = c.customerlastname,
                DateBirth = c.datebirth,
                Dni = c.dni,
                Celphone = c.celphone,
                Address = c.address,
                Gender = c.ismale == true ? Gender.Masculino
                         : c.ismale == false ? Gender.Femenino
                         : Gender.Otro,
                Comentaries = c.comentaries,
                EndDate = c.fechabaja.HasValue ? c.fechabaja.Value : null,
                StartDate = c.fechaalta.HasValue ? c.fechaalta.Value : null,
                Goals = c.customerGoals?
                            .Select(cg => new GoalFindResponseDto
                            {
                                Id = cg.goalid,
                                GoalName = cg.Goal.goalname
                            })
                            .ToList()
                            ?? new List<GoalFindResponseDto>(),
                Memberships = c.customerMembership?.Select(cm => new CustomerMemberShipResponseDto
                {
                    EndDate = cm.enddate.HasValue ? cm.enddate.Value : null,
                    StartDate = cm.startdate,
                    MembershipId = cm.membershipid,
                    MembershipName = cm.Membership.name
                }).ToList() ?? new List<CustomerMemberShipResponseDto>(),
                HealthCustomer= new HealthCustomerDto
                {
                    Alergics = c.healthCustomer.alergics,
                    Height = c.healthCustomer.heigth,
                    MedicalCondicion = c.healthCustomer?.medicalCondicion,
                    Weight = c.healthCustomer.weight
                },
            };

        }

        public async Task<IEnumerable<CustomerFindResponseDto>> GetAllAsync(bool? active)
        {
            var customers = await _customerRepository.GetAllAsync(active);

            return customers.Select(c => new CustomerFindResponseDto
            {
                IdCoustomer = c.id,
                CoustomerName = c.customername,
                CoustomerLastName = c.customerlastname,
                DateBirth = c.datebirth,
                Dni = c.dni,
                Celphone = c.celphone,
                Adress = c.address,
                StartDate = c.fechaalta.HasValue ? c.fechaalta.Value : null,
                EndDate = c.fechabaja.HasValue ? c.fechabaja.Value : null,
                Gender = c.ismale == true ? Gender.Masculino
                : c.ismale == false ? Gender.Femenino
                : Gender.Otro,
                Comentaries = c.comentaries
            });
        }

        public async Task<IEnumerable<ClassCustomerResponseDto>> GetClasses(int id)
        {
            var customers = await _customerRepository.GetClassesAsync(id);

            return customers.Select(c => new ClassCustomerResponseDto
            {
                Capacity = c.capacity,
                ClassId = c.id,
                CustomerId = id,
                Day = c.day,
                Hour = c.hour,
                Name = c.name
            });
        }

        public async Task<bool> UpdateAsync(CustomerUpdateRequestDto customerUpdate)
        {
            var customerExisting = await this._customerRepository.FindByIdAsync(customerUpdate.IdCustomer);
            if (customerExisting == null) throw new InvalidOperationException("No se encontró el cliente");

            if (!string.IsNullOrWhiteSpace(customerUpdate.CustomerName))
                customerExisting.customername = customerUpdate.CustomerName;

            if (!string.IsNullOrWhiteSpace(customerUpdate.CustomerLastName))
                customerExisting.customerlastname = customerUpdate.CustomerLastName;

            if (customerUpdate.DateBirth != default)
                customerExisting.datebirth = customerUpdate.DateBirth.Value;

            if (customerUpdate.Dni.HasValue)
                customerExisting.dni = customerUpdate.Dni.Value;

            if (!string.IsNullOrWhiteSpace(customerUpdate.Celphone))
                customerExisting.celphone = customerUpdate.Celphone;

            if (!string.IsNullOrWhiteSpace(customerUpdate.Address))
                customerExisting.address = customerUpdate.Address;

            if (!string.IsNullOrWhiteSpace(customerUpdate.Comentaries))
                customerExisting.comentaries = customerUpdate.Comentaries;

            if (customerUpdate.IsMale.HasValue)
                customerExisting.ismale = customerUpdate.IsMale;

            
            if (customerUpdate.Health != null)
            {                
                customerExisting.healthCustomer = new HealthCustomer
                {
                    alergics = customerUpdate.Health.Alergics,
                    medicalCondicion = customerUpdate.Health.MedicalCondicion,
                    heigth = customerUpdate.Health.Height,
                    weight = customerUpdate.Health.Weight,
                    customerid = customerExisting.id                    
                };
            }

            
            if (customerUpdate.idGoal != null && customerUpdate.idGoal.Any())
            {
                customerExisting.customerGoals = customerUpdate.idGoal
                    .Where(g => g.HasValue)
                    .Select(g => new CustomerGoal { goalid = g.Value, customerid = customerExisting.id })
                    .ToList();
            }

            
            if (customerUpdate.Memberships != null && customerUpdate.Memberships.Any())
            {
                customerExisting.customerMembership = customerUpdate.Memberships
                    .Where(m => m != null)
                    .Select(m => new CustomerMembership
                    {
                        customerid = customerExisting.id,
                        membershipid = m.IdMembership,
                        startdate = m.StartDate,
                        enddate = m.EndDate                        
                    })
                    .ToList();
            }         
           

           
            var result = await _customerRepository.UpdateAsync(customerExisting);
            return result;
        }

        public Task<bool> AddAbsenceAsync(int customerId, AddCustomerMembershipRequestDto membershipCustomer)
        {
            throw new NotImplementedException();
        }
    }
}
