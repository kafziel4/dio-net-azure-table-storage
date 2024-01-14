using Azure.Data.Tables;
using AzureTableStorageAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace AzureTableStorageAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContatosController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly string _tableName;

        public ContatosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("SAConnectionString")!;
            _tableName = configuration.GetValue<string>("AzureTableName")!;
        }

        [HttpPost]
        public IActionResult Criar(Contato contato)
        {
            var tableClient = GetTableClient();

            contato.RowKey = Guid.NewGuid().ToString();
            contato.PartitionKey = contato.RowKey;

            tableClient.UpsertEntity(contato);

            return Ok(contato);
        }

        [HttpPut("{id}")]
        public IActionResult Atualizar(string id, Contato contato)
        {
            var tableClient = GetTableClient();
            var contatoTable = tableClient.GetEntity<Contato>(id, id).Value;

            contatoTable.Nome = contato.Nome;
            contatoTable.Telefone = contato.Telefone;
            contatoTable.Email = contato.Email;

            tableClient.UpsertEntity(contatoTable);

            return Ok();
        }

        [HttpGet]
        public IActionResult ObterTodos([FromQuery] string? nome)
        {
            var tableClient = GetTableClient();
            List<Contato> contatos;

            if (string.IsNullOrWhiteSpace(nome))
                contatos = [.. tableClient.Query<Contato>()];
            else
                contatos = [.. tableClient.Query<Contato>(c => c.Nome == nome)];

            return Ok(contatos);
        }

        [HttpDelete("{id}")]
        public IActionResult Deletar(string id)
        {
            var tableClient = GetTableClient();
            tableClient.DeleteEntity(id, id);
            return NoContent();
        }

        private TableClient GetTableClient()
        {
            var serviceClient = new TableServiceClient(_connectionString);
            var tableClient = serviceClient.GetTableClient(_tableName);

            tableClient.CreateIfNotExists();
            return tableClient;
        }
    }
}
