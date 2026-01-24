using SanuApi.Application.DTOs.Goal;
using SanuApi.Application.DTOs.Membership;

namespace SanuApi.Application.Interfaces
{
    public interface IMembershipService
    {
        /// <summary>
        /// Inserta una nueva membresía en la base de datos.
        /// </summary>
        /// <param name="membership"></param>
        /// <returns>Valor<see langword="int"/></returns>
        Task<int> AddAsync(MembershipAddRequestDto membership);

        /// <summary>
        /// Actualiza una nueva membresía en la base de datos.
        /// </summary>
        /// <param name="membership"></param>
        /// <returns>Valor<see langword="bool"/></returns>
        Task<bool> UpdateAsync(MembershipUpdateRequestDto request);

        /// <summary>
        /// Busca una membresía por su Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Objeto<see cref="MembershipFindResponseDto"/></returns>
        Task<MembershipFindResponseDto?> FindByIdAsync(int id);

        /// <summary>
        /// Devuelve todas las membresías.
        /// </summary>
        /// <returns>List MembershipFindResponseDto</returns>
        Task<IEnumerable<MembershipFindResponseDto>> GetAllAsync();

        /// <summary>
        /// Elimina una membresía en la base de datos.
        /// </summary>
        /// <param name="membership"></param>
        /// <returns>Valor<see langword="bool"/></returns>
        Task<bool> DeleteAsync(int idMembership);
    }
}
