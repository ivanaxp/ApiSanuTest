using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CustomerService> _logger;
        public CustomerService(ICustomerRepository customerRepository, IHealthCustomerRepository healthCustomerRepository, IGoalRepository goalRepository, ICustomerMembershipRepository customerMembershipRepository, IDbConnection db, ILogger<CustomerService> logger)
        {
            _customerRepository = customerRepository;
            _healthCustomerRepository = healthCustomerRepository;
            _goalRepository = goalRepository;
            _customerMembershipRepository = customerMembershipRepository;
            _db = db;
            _logger = logger;
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
                    if (idMembership <= 0) throw new InvalidOperationException("No se pudo guardar la membres�a del cliente.");
                }

            return true;
        }


        public async Task<int> AddAsync(CustomerAddRequestDto customer)
        {
            if (!customer.Memberships.Any())
                throw new ArgumentException("Debe especificar al menos una members�a.");
            if (!customer.CustomerClasses.Any())
                throw new ArgumentException("Debe especificar al menos una clase.");

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
                    if (idHealth <= 0) throw new InvalidOperationException("No se pudo guardar la condici�n m�dica del cliente.");

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
                        if (idMembership <= 0) throw new InvalidOperationException("No se pudo guardar la membres�a del cliente.");
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

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _customerRepository.FindByIdAsync(id);
            if (customer == null) return false;

            return await _customerRepository.DeleteAsync(id);
        }

        public async Task<CutsomerFindByIdResponseDto?> FindByIdAsync(int id)
        {
            var c = await _customerRepository.FindByIdAsync(id);
            if (c == null)
                return null;

            var classes = await _customerRepository.GetClassesAsync(id);

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
                Classes = classes.Select(cl => new ClassCustomerResponseDto
                {
                    ClassId = cl.id,
                    CustomerId = id,
                    Name = cl.name,
                    Day = cl.day,
                    Hour = cl.hour,
                    Capacity = cl.capacity
                }).ToList()
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
            _logger.LogInformation("[CustomerService.UpdateAsync] Buscando cliente IdCustomer={IdCustomer}", customerUpdate.IdCustomer);
            var customerExisting = await this._customerRepository.FindByIdAsync(customerUpdate.IdCustomer);
            if (customerExisting == null)
            {
                _logger.LogWarning("[CustomerService.UpdateAsync] Cliente no encontrado. IdCustomer={IdCustomer}", customerUpdate.IdCustomer);
                throw new InvalidOperationException("No se encontro el cliente");
            }

            _logger.LogInformation("[CustomerService.UpdateAsync] Cliente encontrado. Aplicando cambios en campos basicos...");

            if (!string.IsNullOrWhiteSpace(customerUpdate.CustomerName))
                customerExisting.customername = customerUpdate.CustomerName;

            if (!string.IsNullOrWhiteSpace(customerUpdate.CustomerLastName))
                customerExisting.customerlastname = customerUpdate.CustomerLastName;

            if (customerUpdate.DateBirth.HasValue)
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

            if (_db.State != ConnectionState.Open)
                _db.Open();
            _logger.LogInformation("[CustomerService.UpdateAsync] Iniciando transaccion...");
            using var transaction = _db.BeginTransaction();
            try
            {
                _logger.LogInformation("[CustomerService.UpdateAsync] Llamando CustomerRepository.UpdateAsync...");
                var result = await _customerRepository.UpdateAsync(customerExisting);
                if (!result) throw new InvalidOperationException("No se pudo actualizar el cliente.");
                _logger.LogInformation("[CustomerService.UpdateAsync] UPDATE customer OK.");

                if (customerUpdate.Health != null)
                {
                    _logger.LogInformation("[CustomerService.UpdateAsync] Upsert HealthCustomer. Height={Height}, Weight={Weight}",
                        customerUpdate.Health.Height, customerUpdate.Health.Weight);
                    await _healthCustomerRepository.UpsertAsync(new HealthCustomer
                    {
                        customerid = customerExisting.id,
                        heigth = customerUpdate.Health.Height,
                        weight = customerUpdate.Health.Weight,
                        alergics = customerUpdate.Health.Alergics,
                        medicalCondicion = customerUpdate.Health.MedicalCondicion
                    });
                    _logger.LogInformation("[CustomerService.UpdateAsync] Upsert HealthCustomer OK.");
                }

                if (customerUpdate.idGoal != null && customerUpdate.idGoal.Any())
                {
                    _logger.LogInformation("[CustomerService.UpdateAsync] Upsert Goals: {Goals}", string.Join(",", customerUpdate.idGoal));
                    foreach (var goalId in customerUpdate.idGoal)
                        await _customerRepository.UpsertGoalAsync(new CustomerGoal { customerid = customerExisting.id, goalid = goalId });
                    _logger.LogInformation("[CustomerService.UpdateAsync] Upsert Goals OK.");
                }

                if (customerUpdate.Memberships != null && customerUpdate.Memberships.Any())
                {
                    _logger.LogInformation("[CustomerService.UpdateAsync] Upsert Memberships: {Count} item(s)", customerUpdate.Memberships.Count());
                    foreach (var m in customerUpdate.Memberships)
                        await _customerMembershipRepository.UpsertAsync(new CustomerMembership
                        {
                            customerid = customerExisting.id,
                            membershipid = m.IdMembership,
                            startdate = m.StartDate,
                            enddate = m.EndDate
                        });
                    _logger.LogInformation("[CustomerService.UpdateAsync] Upsert Memberships OK.");
                }

                if (customerUpdate.CustomerClasses != null)
                {
                    _logger.LogInformation("[CustomerService.UpdateAsync] Eliminando clases existentes del cliente Id={Id}", customerExisting.id);
                    await _customerRepository.DeleteClassesAsync(customerExisting.id);

                    foreach (var classId in customerUpdate.CustomerClasses)
                    {
                        var idClassCustomer = await _customerRepository.AddClassesAsync(new ClassCustomer { customerid = customerExisting.id, classid = classId });
                        if (idClassCustomer <= 0) throw new InvalidOperationException("No se pudo guardar la clase del cliente.");
                    }
                    _logger.LogInformation("[CustomerService.UpdateAsync] Clases actualizadas: {Classes}", string.Join(",", customerUpdate.CustomerClasses));
                }

                transaction.Commit();
                _logger.LogInformation("[CustomerService.UpdateAsync] Transaccion committed. Exito.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CustomerService.UpdateAsync] ERROR en transaccion. Haciendo rollback. Mensaje: {Message}. InnerException: {Inner}",
                    ex.Message, ex.InnerException?.Message);
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> AddAbsenceAsync(int customerId, AddCustomerAbsenceRequestDto absence)
        {
            var validStatuses = new[] { "presente", "ausente", "ausente_justificado" };
            var status = absence.Status?.ToLower();
            if (status != null && !validStatuses.Contains(status))
                throw new ArgumentException($"Estado inválido. Los valores permitidos son: {string.Join(", ", validStatuses)}.");

            var result = await _customerRepository.AddAbsenceAsync(new Absences
            {
                classid = absence.IdClass,
                customerid = absence.IdCustomer,
                dateabsence = absence.DateAbsence,
                status = status ?? "ausente"
            });
            return result > 0;
        }

        public async Task<IEnumerable<CustomerAbsenceResponseDto>> GetAbsencesAsync(int customerId)
        {
            var absences = await _customerRepository.GetAbsencesAsync(customerId);

            return absences.Select(a => new CustomerAbsenceResponseDto
            {
                Id = a.Absence.id,
                ClassId = a.Absence.classid,
                ClassName = a.Class?.name,
                DateAbsence = a.Absence.dateabsence,
                Status = a.Absence.status
            });
        }
    }
}
