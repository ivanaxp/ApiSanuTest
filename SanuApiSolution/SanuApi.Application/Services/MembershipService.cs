
using SanuApi.Application.DTOs.Membership;
using SanuApi.Application.Interfaces;
using SanuApi.Domain.Entities;
using SanuApi.Domain.Interfaces;

namespace SanuApi.Application.Services
{
    public class MembershipService : IMembershipService
    {
        private readonly IMembershipRepository membershipRepository;

        public MembershipService( IMembershipRepository membershipRepository)
        {
          this.membershipRepository = membershipRepository;
        }

        public async Task<MembershipFindResponseDto?> FindByIdAsync(int id)
        {
            var membership = membershipRepository.FindByIdAsync(id);
            return await membership.ContinueWith(t =>
            {
                var g = t.Result;
                if (g == null) return null;
                return new MembershipFindResponseDto
                {
                    Id= g.id,
                    Name = g.name,
                    Price = g.price,
                    Frecuency = g.frecuency
                };
            });
        }
        public async Task<int> AddAsync(MembershipAddRequestDto membership)
        {
            var newMembership = new Membership
            {
                name = membership.Name,
                price = membership.Price,
                frecuency = membership.Frecuency
            };
           
            return await membershipRepository.AddAsync(newMembership);
        }

        public async Task<IEnumerable<MembershipFindResponseDto>> GetAllAsync()
        {
            var memberships = await membershipRepository.GetAllAsync();
            return memberships.Select(g => new MembershipFindResponseDto
            {
                Id = g.id,
                Name = g.name,
                Price = g.price,
                Frecuency = g.frecuency

            });
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
            return await membershipRepository.UpdateAsync(membershipToUpdate);
        }

        public async Task<bool> DeleteAsync(int idMembership)
        {
            return await membershipRepository.DeleteAsync(idMembership);
        }
    }
}
