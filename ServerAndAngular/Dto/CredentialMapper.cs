using SoupDiscover.Dto;
using SoupDiscover.ORM;
using System.Collections.Generic;

namespace SoupDiscover.Controllers
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
            };
        }

        public static CredentialDto ToDto(this Credential model)
        {
            if(model == null)
            {
                return null;
            }
            return new CredentialDto()
            {
                Name = model.Name,
                Key = model.Key,
            };
        }

        public static IEnumerable<CredentialDto> ToDto(this IEnumerable<Credential> credentials)
        {
            if (credentials == null)
            {
                yield break;
            }
            foreach(var c in credentials)
            {
                yield return new CredentialDto()
                {
                    Name = c.Name,
                    Key = "*****",
                };
            }
        }
    }
}
