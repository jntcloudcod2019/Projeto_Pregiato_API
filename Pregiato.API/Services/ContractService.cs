using Pregiato.API.Data;
using Pregiato.API.Interface;
using Pregiato.API.Models;

namespace Pregiato.API.Services
{
    public class ContractService : IContractService
    {
       public  string GenerateContractPdf(ContractDto dto)
        {
           var contractContent = $@"
            CONTRATO DE PRESTAÇÃO DE SERVIÇOS E PRODUÇÃO FOTOGRÁFICA

            Presidente Prudente, {DateTime.Now:dddd, dd de MMMM de yyyy}

            CONTRATANTE: {dto.NomeModelo}, inscrito(a) no CPF: {dto.CPFModelo} e portador da cédula de RG: {dto.RGModelo}, residente no endereço {dto.EnderecoModelo}, Nº {dto.NumeroModelo} - {dto.ComplementoModelo}, localizado no bairro {dto.BairroModelo}, situado na cidade de {dto.CidadeModelo}, CEP: {dto.CEPModelo}, com telefone principal: {dto.TelefonePrincipal} e telefone secundário:.

            CONTRATADA: WIKI PRODUÇÕES E TREINAMENTOS, situada á Avenida Paulista, Nº 1636 - Conj. 1105 - Bela Vista - CEP: 01310-100, São Paulo - SP.

            CLÁUSULA 1º - OBJETO
            ... (restante do contrato)
                                       ";
           byte[] pdfBytes = System.Text.Encoding.UTF8.GetBytes(contractContent);
           return Convert.ToBase64String(pdfBytes);
        }
    }
}
