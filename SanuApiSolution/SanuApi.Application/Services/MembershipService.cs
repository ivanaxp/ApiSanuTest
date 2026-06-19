
using SanuApi.Application.DTOs.Class;
using SanuApi.Application.DTOs.Membership;
using SanuApi.Application.Interfaces;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;

namespace SanuApi.Application.Services
{
    public class MembershipService : IMembershipService
    {
        private readonly IMembershipRepository _membershipRepository;
        private readonly IClassRepository _classRepository;

        public MembershipService(IMembershipRepository membershipRepository, IClassRepository classRepository)
        {
            _membershipRepository = membershipRepository;
            _classRepository = classRepository;
        }

        public async Task<MembershipFindResponseDto?> FindByIdAsync(int id)
        {
            var membership = await _membershipRepository.FindByIdAsync(id);
            if (membership == null) return null;

            var classes = await _classRepository.GetByMembershipIdAsync(id);
            return MapToDto(membership, classes);
        }

        public async Task<int> AddAsync(MembershipAddRequestDto membership)
        {
            var newMembership = new Membership
            {
                name = membership.Name,
                price = membership.Price,
                frecuency = membership.Frecuency
            };
            return await _membershipRepository.AddAsync(newMembership);
        }

        public async Task<IEnumerable<MembershipFindResponseDto>> GetAllAsync()
        {
            var memberships = await _membershipRepository.GetAllAsync();
            var result = new List<MembershipFindResponseDto>();

            foreach (var m in memberships)
            {
                var classes = await _classRepository.GetByMembershipIdAsync(m.id);
                result.Add(MapToDto(m, classes));
            }

            return result;
        }

        public async Task<bool> UpdateAsync(MembershipUpdateRequestDto request)
        {
            var membershipToUpdate = new Membership
            {
                id = request.Id,
                name = request.Name,
                price = request.Price,
                frecuency = request.Frecuency
            };
            return await _membershipRepository.UpdateAsync(membershipToUpdate);
        }

        public async Task<bool> DeleteAsync(int idMembership)
        {
            return await _membershipRepository.DeleteAsync(idMembership);
        }

        private static MembershipFindResponseDto MapToDto(Membership m, IEnumerable<Classes> classes)
        {
            var classList = classes.ToList();
            return new MembershipFindResponseDto
            {
                Id = m.id,
                Name = m.name,
                Price = m.price,
                Frecuency = m.frecuency,
                Classes = classList.Count > 0
                    ? classList.Select(c => new ClassInMembershipDto
                    {
                        Id = c.id,
                        Name = c.name,
                        Dates = c.Dates.Select(d => new ClassDateResponseDto
                        {
                            Day = d.day,
                            Hour = d.hour,
                            Capacity = d.capacity
                        }).ToList()
                    }).ToList()
                    : null
            };
        }
    }
}
