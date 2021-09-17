using SoupDiscover.ORM;
using System.Collections.Generic;

namespace SoupDiscover.Dto
{
    public static class CredentialMapper
    {
        public static Credential ToModel(this CredentialDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            return new Credential()
            {
                Name = dto.Name,
                Key = dto.Key,
                Login = dto.Login,
                Password = dto.Password,
                CredentialType = dto.CredentialType,
                Token = dto.Token,
            };
        }

        public static CredentialDto ToDto(this Credential model, bool securised = false)
        {
            if (model == null)
            {
                return null;
            }
            return new CredentialDto()
            {
                Name = model.Name,
                Key = securised ? "*****" : model.Key,
                Login = model.Login,
                Password = securised ? "*****" : model.Password,
                CredentialType = model.CredentialType,
                Token = securised ? "*****" : model.Token,
            };
        }

        public static IEnumerable<CredentialDto> ToDto(this IEnumerable<Credential> credentials, bool securised)
        {
            if (credentials == null)
            {
                yield break;
            }
            foreach (var c in credentials)
            {
                yield return c.ToDto(securised);
            }
        }
    }
}
