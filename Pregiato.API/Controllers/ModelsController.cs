using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Response;
using Swashbuckle.AspNetCore.Annotations;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Pregiato.API.DTO;
using Pregiato.API.Interfaces;
using iText.Commons.Actions.Contexts;
using iText.Kernel.Pdf.Colorspace.Shading;
using PuppeteerSharp;
using Pregiato.API.Services.ServiceModels;
using System.Text.Json.Serialization;

namespace Pregiato.API.Controllers
{
    using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
    using Alias = string;

    [ApiController]
    [Route("api/[controller]")]
    public class ModelsController : ControllerBase
    {
        private readonly IModelRepository _modelRepository;
        private readonly IUserService _userService;
        private readonly IServiceUtilites _serviceUtilites;
        private readonly IUserRepository _userRepository;
        private readonly IProducersRepository _producersRepository;
        private readonly ModelAgencyContext _agencyContext;
        private readonly CustomResponse _customResponse;
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory;

        public ModelsController
              (CustomResponse customResponse,
               ModelAgencyContext agencyContext,
               IModelRepository modelRepository,
               IContractService contractService,
               IJwtService jwtService,
               IUserService userService,
               IServiceUtilites serviceUtilites,
               IUserRepository userRepository,
               IProducersRepository producersRepository,
               IDbContextFactory<ModelAgencyContext> contextFactory)
        {
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _agencyContext = agencyContext ?? throw new ArgumentNullException(nameof(agencyContext));
            ;
            _userService = userService;
            _serviceUtilites = serviceUtilites;
            _userRepository = userRepository;
            _customResponse = customResponse;
            _contextFactory = contextFactory;
            _producersRepository = producersRepository;
        }


        [Authorize(Policy = "GlobalPolitics")]
        [HttpGet("GetAllModels")]
        [SwaggerOperation("Retorna todos os modelos cadastrados.")]
        public async Task<IActionResult> GetAllModels()
        {

            try
            {
                IEnumerable<Model> modelsExists = await _modelRepository.GetAllModelAsync().ConfigureAwait(true);


                var resultModels = modelsExists.Select(model =>
                {
                    ModelDnaData attributes;

                    if (model.DNA != null)
                    {
                        try
                        {
                            var options = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true,
                                AllowTrailingCommas = true,
                                ReadCommentHandling = JsonCommentHandling.Skip,
                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                            };

                             attributes = JsonSerializer.Deserialize<ModelDnaData>(model.DNA.RootElement.GetRawText(), options);
                        }
                        catch
                        {
                            attributes = new ModelDnaData();
                        }
                    }
                    else
                    {
                        attributes = new ModelDnaData();
                    }



                    attributes.Appearance ??= new Appearance
                    {
                        Eyes = new EyeAttributes(),
                        Hair = new HairAttributes(),
                        Skin = new SkinAttributes { Marks = new List<string>() },
                        Face = new FaceAttributes(),
                        Smile = new SmileAttributes(),
                        Body = new BodyAttributes()
                    };
                    attributes.EyeAttributes ??= new EyeAttributes();
                    attributes.HairAttributes ??= new HairAttributes();
                    attributes.SkinAttributes ??= new SkinAttributes { Marks = new List<string>() };
                    attributes.FaceAttributes ??= new FaceAttributes();
                    attributes.SmileAttributes ??= new SmileAttributes();
                    attributes.BodyAttributes ??= new BodyAttributes();
                    attributes.AdditionalAttributes ??= new AdditionalAttributes
                    {
                        Skills = new List<string>(),
                        Experience = new List<string>()
                    };
                    attributes.PhysicalCharacteristics ??= new PhysicalCharacteristics();

                    var attributesJson = JsonSerializer.SerializeToDocument(attributes);

                    return new ResulModelsResponse
                    {
                        ID = model.IdModel.ToString(),
                        NAME = model.Name,
                        CPF = model.CPF,
                        RG = model.RG,
                        DATEOFBIRTH = model.DateOfBirth,
                        EMAIL = model.Email,
                        AGE = model.Age,
                        MODELATTRIBUTES = attributesJson, 
                        TELEFONEPRINCIPAL = model.TelefonePrincipal,
                        STATUS = model.Status ? "ATIVO" : "DESCONTINUADO",
                        RESPONSIBLEPRODUCER = model.CodProducers,
                        
                        ADRESSINFO = new AdressInfo
                        {
                            ADDRESS = model.Address,
                            NUMBERADDRESS = model.NumberAddress,
                            POSTALCODE = model.PostalCode,
                            CITY = model.City,
                            UF = model.UF
                        },
                    };
                }).ToList();


                return Ok(new ModelsResponse
                {
                    SUCESS = true,
                    DATA = resultModels
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "OCORREU UM ERRO AO BUSCAR OS MODELOS.",
                    error = new
                    {
                        code = "INTERNAL_SERVER_ERROR",
                        details = ex.Message
                    }
                });
            }

        }

        [Authorize(Policy = "GlobalPolitics")]
        [HttpPost("AddModels")]
        [SwaggerOperation("Criar novo modelo.")]
        [ProducesResponseType(typeof(CustomResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CustomResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CustomResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddNewModel([FromBody] CreateModelRequest createModelRequest)
        {
           
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |Validando se model {createModelRequest.Name} | Documento: {createModelRequest.CPF}");

            ModelCheckDto? checkExistence = await _modelRepository.GetModelCheck(createModelRequest)
                                                                  .ConfigureAwait(true);

            if (checkExistence != null)
            {
                Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Modelo: {createModelRequest.Name} já cadastrado.. ");

                return BadRequest(new CustomResponse
                {
                    StatusCode = StatusCodes.Status304NotModified,
                    Message = $"Modelo {createModelRequest.Name} já cadastrado.",
                    Data = null,
                });
            }

            User producer = await _userRepository.GetByProducersAsync(createModelRequest.Nameproducers)
                                                 .ConfigureAwait(true);

            var defaultDnaData = new ModelDnaData
            {
                Dna = "DNA",
                Appearance = new Appearance
                {
                    Eyes = new EyeAttributes(),
                    Hair = new HairAttributes(),
                    Skin = new SkinAttributes { Marks = new List<string>() },
                    Face = new FaceAttributes(),
                    Smile = new SmileAttributes(),
                    Body = new BodyAttributes()
                },
                EyeAttributes = new EyeAttributes(),
                HairAttributes = new HairAttributes(),
                SkinAttributes = new SkinAttributes { Marks = new List<string>() },
                FaceAttributes = new FaceAttributes(),
                SmileAttributes = new SmileAttributes(),
                BodyAttributes = new BodyAttributes(),
                AdditionalAttributes = new AdditionalAttributes
                {
                    Skills = new List<string>(),
                    Experience = new List<string>()
                },
                PhysicalCharacteristics = new PhysicalCharacteristics()
            };

           
            Model model = new Model
            {
                CodProducers = producer.CodProducers,
                Name = createModelRequest.Name,
                CPF = createModelRequest.CPF,
                RG = createModelRequest.RG,
                DateOfBirth = createModelRequest.DateOfBirth,
                Age = await _serviceUtilites
                    .CalculateAge(createModelRequest.DateOfBirth ?? DateTime.MinValue)
                    .ConfigureAwait(true),
                Email = createModelRequest.Email,
                PostalCode = createModelRequest.PostalCode,
                Address = createModelRequest.Address,
                NumberAddress = createModelRequest.NumberAddress,
                Complement = createModelRequest.Complement,
                BankAccount = createModelRequest.BankAccount,
                Neighborhood = createModelRequest.Neighborhood,
                City = createModelRequest.City,
                UF = createModelRequest.UF,
                Status = true,
                DNA = JsonDocument.Parse(JsonSerializer.Serialize(defaultDnaData)),
                TelefonePrincipal = createModelRequest.TelefonePrincipal,
                TelefoneSecundario = createModelRequest.TelefoneSecundario

            };

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Processando cadastro do Moelo: {model.Name} | Documento:{model.CPF}. ");
            await _modelRepository.AddModelAsync(model).ConfigureAwait(true);

            Console.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Modelo cadastro: {model.Name} | Documento:{model.CPF}. ");
            await _userService.RegisterUserModelAsync(createModelRequest.Name, createModelRequest.Email, producer.CodProducers);

            return Ok(new ModelResponse
            {
                Mensage = "CADASTRO DO MODELO CRIADO COM SUCESSO.",
                Model = new ModelInfo
                {
                    Name = model.Name
                }
            });
        }

        [Authorize(Policy = "ManagementPolicyLevel2")]
        [HttpDelete("DeleteModel{id}")]
        [SwaggerOperation("Deletar cadastro de modelos.")]
        public async Task<IActionResult> DeleteModel(Guid id)
        {
            Model? modelExists = await _modelRepository.GetByIdModelAsync(id);
            if (modelExists == null)
            {
                return NotFound();
            }
            await _modelRepository.DeleteModelAsync(id).ConfigureAwait(true);
            return NoContent();
        }

        [Authorize(Policy = "PolicyModels")]
        [HttpGet("modelFeedJobs")]
        public async Task<IActionResult> GetModelFeed()
        {
            var username = await _userService
                 .UserCaptureByToken()
                 .ConfigureAwait(true);
                

            var model = await _agencyContext.Models
                .FirstOrDefaultAsync(m => m.Email== username.Email)
                .ConfigureAwait(true);


            var feed = await _agencyContext.ModelJobs
           .Where(mj => mj.ModelId == model.IdModel )
           .Select(mj => new
           {
               mj.ModelId,
               mj.JobDate,
               mj.Location,
               mj.Time,
               mj.AdditionalDescription
           })
           .ToListAsync();

            return Ok(new
            {
                Feed = feed
            });
        }

        [Authorize(Policy = "GlobalPoliticsAgency")]
        [HttpGet("findModel")]
        public async Task<IActionResult> FindModel(string query)
        {

            try
            {
                Model model = await _modelRepository.GetModelByCriteriaAsync(query);

                if (model == null)
                {
                    return Ok(new ModelsResponse
                    {
                        SUCESS = false,
                        MESSAGE = $"MODELO COM {query.ToUpper()} NÃO ENCONTRADO.",
                        DATA = null
                    });
                }


                var resultDNA = model.DNA.Deserialize<ModelDnaData>() ?? new ModelDnaData();

                var producer = await _producersRepository.GetProducersAsync(model.CodProducers);

                var user = await _userRepository.GetByUsernameAsync(model.Email).ConfigureAwait(true);

                var resultModel = new ResulModelsResponse
                {
                    ID = model.IdModel.ToString(),
                    IDUSER = user.UserId.ToString(),
                    NAME = model.Name,
                    CPF = model.CPF,
                    RG = model.RG,
                    DATEOFBIRTH = model.DateOfBirth,
                    EMAIL = model.Email,
                    AGE = model.Age,
                    TELEFONEPRINCIPAL = model.TelefonePrincipal,
                    STATUS = model.Status ? "ATIVO" : "DESCONTINUADO",
                    RESPONSIBLEPRODUCER = producer.Name.ToUpper() ,
                    ADRESSINFO = string.IsNullOrEmpty(model.Address) && string.IsNullOrEmpty(model.City) && string.IsNullOrEmpty(model.UF)
                        ? null
                        : new AdressInfo
                        {
                            ADDRESS = model.Address,
                            NUMBERADDRESS = model.NumberAddress,
                            POSTALCODE = model.PostalCode,
                            CITY = model.City,
                            UF = model.UF
                        },
                    MODELATTRIBUTES =  JsonDocument.Parse(JsonSerializer.Serialize(resultDNA)),

                };
                
                return Ok(new ModelsResponse
                {
                    SUCESS = true,
                    DATA = new List<ResulModelsResponse> { resultModel } 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ocorreu um erro ao buscar o modelo.",
                    error = new
                    {
                        code = "INTERNAL_SERVER_ERROR",
                        details = ex.Message
                    }
                });
            }
        }

        [Authorize(Policy = "PolicyModels")]
        [HttpGet("my-contracts")]
        public async Task<IActionResult> GetMyContracts()
        {

            try
            {
                var username = await _userService.UserCaptureByToken().ConfigureAwait(true);

                var schforModel = await _userRepository.GetByUsernameAsync(username.Email).ConfigureAwait(true);
                if (schforModel is null)
                {
                    return BadRequest("USUÁRIO NÃO ENCONTRADO NA BASE DE DADOS.");
                }

                var model = await _agencyContext.Models
                    .FirstOrDefaultAsync(m => m.Email == schforModel.Email)
                    .ConfigureAwait(true);

                if (model is null)
                {
                    return BadRequest("MODELO NÃO ENCONTRADO PARA O USUÁRIO.");
                }

                var contracts = await _agencyContext.Contracts
                    .Where(c => c.IdModel == model.IdModel)
                    .ToListAsync().ConfigureAwait(true);

                if (!contracts.Any())
                {
                    return ActionResultIndex.Failure("NENHUM CONTRATO ENCONTRADO NA BASE DE DADOS.");
                }

                var contractsDto = contracts.Select(c => new ContractSummaryDTO
                {
                    ContractId = c.ContractId,
                    ModelId = c.IdModel,
                    DataContrato = c.DataContrato,
                    VigenciaContrato = c.VigenciaContrato,
                    ValorContrato = c.ValorContrato,
                    FormaPagamento = c.FormaPagamento,
                    StatusPagamento = c.StatusPagamento,
                    ContractFilePath = c.ContractFilePath,
                    CodProposta = c.CodProposta,
                    CodProduces = c.CodProducers
                }).ToList();

                return ActionResultIndex.Success(
                    data: new
                    {
                        TotalContracts = contractsDto.Count,
                        Contracts = contractsDto
                    },
                    message: "TODOS OS CONTRATOS FORAM RECUPERADOS COM SUCESSO!"
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO AO RECUPERAR CONTRATOS: {ex.Message.ToUpper()}");
                return ActionResultIndex.Failure($"ERRO AO RECUPERAR OS CONTRATOS: {ex.Message.ToUpper()}",
                isSpeakOnOperation: true);
            }
        }
        
        [Authorize(Policy = "GlobalPoliticsAgency")]
        [HttpPut("update-dna-property/{idModel}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateDnaData([FromRoute] Guid idModel, [FromBody] ModelDnaData requestDNA)
        {
            if (requestDNA == null)
            {
                return BadRequest("DADOS INVÁLIDOS.");
            }

            string jsonDna = JsonSerializer.Serialize(requestDNA, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            JsonDocument dnaDocument;
            try
            {
                dnaDocument = JsonDocument.Parse(jsonDna);
            }
            catch (JsonException ex)
            {
                return BadRequest($"ERRO AO PROCESSAR O JSON: {ex.Message}");
            }

            using var context = _contextFactory.CreateDbContext();

            var model = await context.Models
                            .FirstOrDefaultAsync(m => m.IdModel == idModel)
                            .ConfigureAwait(true);

            if (model == null)
            {
                return NotFound("MODELO NÃO ENCONTRADO COM O ID INFORMADO.");
            }

            model.DNA = dnaDocument;
            model.UpdatedAt = DateTime.UtcNow;
            context.Entry(model).Property(m => m.DNA).IsModified = true;

            await context.SaveChangesAsync().ConfigureAwait(true);

            return NoContent(); 
        }

        [Authorize(Policy = "GlobalPoliticsAgency")]
        [HttpPut("update-dna-property")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateDnaDataAuthenticated ([FromBody] ModelDnaData requestDNA)
        {

            if (requestDNA == null)
            {
                return BadRequest("Dados inválidos.");
            }

            var user = await _userService.UserCaptureByToken().ConfigureAwait(true);
            if (user == null)
            {
                return BadRequest("USUÁRIO NÃO ENCONTRADO.");
            }

            string jsonDna = JsonSerializer.Serialize(requestDNA, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,

            });

            JsonDocument dnaDocument;
            try
            {
                dnaDocument = JsonDocument.Parse(jsonDna);
            }
            catch (JsonException ex)
            {
                return BadRequest($"ERRO AO PROCESSAR O JSON: {ex.Message}");
            }


            using ModelAgencyContext context = _contextFactory.CreateDbContext();

            var model = await context.Models
                .FirstOrDefaultAsync(m => m.Email == user.Email).ConfigureAwait(true);

            if (model == null)
            {
                return NotFound("MODELO NÃO ENCONTRADO PARA O E-MAIL DO USUÁRIO LOGADO.");
            }

            model.DNA = dnaDocument;
            model.UpdatedAt = DateTime.UtcNow;
            context.Entry(model).Property(m => m.DNA).IsModified = true;
            await context.SaveChangesAsync().ConfigureAwait(true);
            return Ok();
        }

        [Authorize(Policy = "GlobalPoliticsAgency")]
        [HttpPost("uploadPhotoModel")]
        public async Task<IActionResult> UploadPhotos([FromForm] ModelPhotoUploadDto dto)
        {

            if (dto.File == null || dto.File.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ARQUIVO INVÁLIDO OU AUSENTE."
                });
            }

            var modelExists = await _modelRepository.GetModelByCriteriaAsync
                (dto.ModelId.ToString())
                .ConfigureAwait(true);

            if (modelExists == null)
                return NotFound(new
                {
                    success = false,
                    message = "MODELO NÃO ENCONTRADO."
                });


            using var ms = new MemoryStream();
            await dto.File.CopyToAsync(ms).ConfigureAwait(true);

            var photo = new ModelPhoto
            {
                Id = Guid.NewGuid(),
                ModelId = dto.ModelId,
                ImageData = ms.ToArray(),
                ImageName = dto.File.FileName,
                ContentType = dto.File.ContentType
            };

            photo.UploadedAt = DateTime.UtcNow;
            await _agencyContext.ModelPhotos.AddAsync(photo).ConfigureAwait(true);
            await _agencyContext.SaveChangesAsync().ConfigureAwait(true);

            return Ok(new PhotoModelResponse
            {
                Id = photo.Id,
                ImageName = photo.ImageName,
                ContentType = photo.ContentType,
                ImageBase64 = $"data:{photo.ContentType};base64,{Convert.ToBase64String(photo.ImageData!)}"

            });
        }

        [Authorize(Policy = "GlobalPoliticsAgency")]
        [HttpGet("GetPhotoModel/{modelId}")]
        public async Task<IActionResult> GetByModel(Guid modelId)
        {
            var photos = await _agencyContext.ModelPhotos
                .Where(p => p.ModelId == modelId && p.ImageData != null)
                .ToListAsync()
                .ConfigureAwait(false);

            if (!photos.Any())
            {
                return NotFound("Nenhuma foto encontrada para o modelo informado.");
            }

            var response = photos.Select(p => new PhotoModelResponse
            {
                Id = p.Id,
                ImageName = p.ImageName,
                ContentType = p.ContentType,
                UploadedAt = p.UploadedAt,
                ImageBase64 = $"data:{p.ContentType};base64,{Convert.ToBase64String(p.ImageData!)}"
 
            }).ToList();

            return Ok(response);
        }

        [Authorize(Policy = "GlobalPoliticsAgency")]
        [HttpPatch("EditeRegisterModel/{id}")]
        public async Task<IActionResult> UpdatePartial(Guid id, [FromBody] UpdateModelPartialDto dto)
        {
           
            var exists = await _agencyContext.Models.AnyAsync(m => m.IdModel == id);
            if (!exists)
            {
                return NotFound(new
                {
                    success = false,
                    message = "MODELO NÃO ENCONTRADO."
                });
            }

            var model = new Model { IdModel = id };
            _agencyContext.Attach(model);

            string PlaceholderString = "string";

            if (dto.Name != null  && dto.Name != PlaceholderString)
            {
                model.Name = dto.Name;
                _agencyContext.Entry(model).Property(m => m.Name).IsModified = true;
            }

            if (dto.Email != null && dto.Email != PlaceholderString)
            {
                model.Email = dto.Email;
                _agencyContext.Entry(model).Property(m => m.Email).IsModified = true;
            }

            if (dto.CPF != null && dto.CPF != PlaceholderString)
            {
                model.CPF = dto.CPF;
                _agencyContext.Entry(model).Property(m => m.CPF).IsModified = true;
            }

            if (dto.RG != null && dto.RG != PlaceholderString)
            {
                model.RG = dto.RG;
                _agencyContext.Entry(model).Property(m => m.RG).IsModified = true;
            }

            if (dto.PostalCode != null && dto.PostalCode != PlaceholderString)
            {
                model.PostalCode = dto.PostalCode;
                _agencyContext.Entry(model).Property(m => m.PostalCode).IsModified = true;
            }

            if (dto.Address != null && dto.Address != PlaceholderString)
            {
                model.Address = dto.Address;
                _agencyContext.Entry(model).Property(m => m.Address).IsModified = true;
            }

            if (dto.NumberAddress != null && dto.NumberAddress != PlaceholderString)
            {
                model.NumberAddress = dto.NumberAddress;
                _agencyContext.Entry(model).Property(m => m.NumberAddress).IsModified = true;
            }

            if (dto.Complement != null && dto.Complement != PlaceholderString)
            {
                model.Complement = dto.Complement;
                _agencyContext.Entry(model).Property(m => m.Complement).IsModified = true;
            }

            if (dto.Neighborhood != null && dto.Neighborhood != PlaceholderString)
            {
                model.Neighborhood = dto.Neighborhood;
                _agencyContext.Entry(model).Property(m => m.Neighborhood).IsModified = true;
            }

            if (dto.City != null && dto.City != PlaceholderString)
            {
                model.City = dto.City;
                _agencyContext.Entry(model).Property(m => m.City).IsModified = true;
            }

            if (dto.TelefonePrincipal != null && dto.TelefonePrincipal != PlaceholderString)
            {
                model.TelefonePrincipal = dto.TelefonePrincipal;
                _agencyContext.Entry(model).Property(m => m.TelefonePrincipal).IsModified = true;
            }

            if (dto.TelefoneSecundario != null && dto.TelefoneSecundario != PlaceholderString)
            {
                model.TelefoneSecundario = dto.TelefoneSecundario;
                _agencyContext.Entry(model).Property(m => m.TelefoneSecundario).IsModified = true;
            }

            if (dto.Age.HasValue && dto.Age > 0)
            {
                model.Age = dto.Age.Value;
                _agencyContext.Entry(model).Property(m => m.Age).IsModified = true;
            }

            model.UpdatedAt = DateTime.UtcNow;
            _agencyContext.Entry(model).Property(m => m.UpdatedAt).IsModified = true;

            var changes = await _agencyContext.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"MODELO ATUALIZADO COM SUCESSO ({changes} CAMPO(S) ALTERADO(S))."
            });
        }
    }
}

