using System.Text.Json.Serialization;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// Request to execute a SQL statement on Databricks SQL Warehouse
    /// </summary>
    public class DatabricksSqlStatementRequest
    {
        /// <summary>
        /// SQL Warehouse ID to execute the statement on
        /// </summary>
        [JsonPropertyName("warehouse_id")]
        public string WarehouseId { get; set; } = null!;

        /// <summary>
        /// SQL statement to execute
        /// </summary>
        [JsonPropertyName("statement")]
        public string Statement { get; set; } = null!;

        /// <summary>
        /// Optional catalog name (default: spark_catalog)
        /// </summary>
        [JsonPropertyName("catalog")]
        public string? Catalog { get; set; }

        /// <summary>
        /// Optional schema name (default: default)
        /// </summary>
        [JsonPropertyName("schema")]
        public string? Schema { get; set; }

        /// <summary>
        /// Wait timeout in seconds (5-50, default: 10)
        /// </summary>
        [JsonPropertyName("wait_timeout")]
        public string? WaitTimeout { get; set; } = "10s";

        /// <summary>
        /// Result format: JSON_ARRAY, ARROW_STREAM, CSV
        /// </summary>
        [JsonPropertyName("format")]
        public string? Format { get; set; } = "JSON_ARRAY";

        /// <summary>
        /// Disposition: INLINE or EXTERNAL_LINKS
        /// </summary>
        [JsonPropertyName("disposition")]
        public string? Disposition { get; set; } = "INLINE";
    }

    /// <summary>
    /// Response from Databricks SQL statement execution
    /// </summary>
    public class DatabricksSqlStatementResponse
    {
        /// <summary>
        /// Unique identifier for the statement execution
        /// </summary>
        [JsonPropertyName("statement_id")]
        public string StatementId { get; set; } = null!;

        /// <summary>
        /// Execution status
        /// </summary>
        [JsonPropertyName("status")]
        public DatabricksSqlStatus Status { get; set; } = null!;

        /// <summary>
        /// Manifest with schema and chunk information
        /// </summary>
        [JsonPropertyName("manifest")]
        public DatabricksSqlManifest? Manifest { get; set; }

        /// <summary>
        /// Query result data
        /// </summary>
        [JsonPropertyName("result")]
        public DatabricksSqlResult? Result { get; set; }
    }

    /// <summary>
    /// Status of SQL statement execution
    /// </summary>
    public class DatabricksSqlStatus
    {
        /// <summary>
        /// Execution state: PENDING, RUNNING, SUCCEEDED, FAILED, CANCELED, CLOSED
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; } = null!;

        /// <summary>
        /// Error information if failed
        /// </summary>
        [JsonPropertyName("error")]
        public DatabricksSqlError? Error { get; set; }
    }

    /// <summary>
    /// Error details from failed statement execution
    /// </summary>
    public class DatabricksSqlError
    {
        /// <summary>
        /// Error message
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;

        /// <summary>
        /// Error code
        /// </summary>
        [JsonPropertyName("error_code")]
        public string? ErrorCode { get; set; }
    }

    /// <summary>
    /// Manifest with schema and chunk information
    /// </summary>
    public class DatabricksSqlManifest
    {
        /// <summary>
        /// Result format
        /// </summary>
        [JsonPropertyName("format")]
        public string Format { get; set; } = null!;

        /// <summary>
        /// Schema definition
        /// </summary>
        [JsonPropertyName("schema")]
        public DatabricksSqlSchema? Schema { get; set; }

        /// <summary>
        /// Total number of rows
        /// </summary>
        [JsonPropertyName("total_row_count")]
        public long? TotalRowCount { get; set; }

        /// <summary>
        /// Total number of chunks
        /// </summary>
        [JsonPropertyName("total_chunk_count")]
        public int? TotalChunkCount { get; set; }

        /// <summary>
        /// Chunks for pagination
        /// </summary>
        [JsonPropertyName("chunks")]
        public List<DatabricksSqlChunk>? Chunks { get; set; }
    }

    /// <summary>
    /// Schema definition for result set
    /// </summary>
    public class DatabricksSqlSchema
    {
        /// <summary>
        /// Column definitions
        /// </summary>
        [JsonPropertyName("columns")]
        public List<DatabricksSqlColumn>? Columns { get; set; }
    }

    /// <summary>
    /// Column definition
    /// </summary>
    public class DatabricksSqlColumn
    {
        /// <summary>
        /// Column name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Column type
        /// </summary>
        [JsonPropertyName("type_name")]
        public string TypeName { get; set; } = null!;
    }

    /// <summary>
    /// Chunk information for pagination
    /// </summary>
    public class DatabricksSqlChunk
    {
        /// <summary>
        /// Chunk index
        /// </summary>
        [JsonPropertyName("chunk_index")]
        public int ChunkIndex { get; set; }

        /// <summary>
        /// Row offset
        /// </summary>
        [JsonPropertyName("row_offset")]
        public long RowOffset { get; set; }

        /// <summary>
        /// Row count in this chunk
        /// </summary>
        [JsonPropertyName("row_count")]
        public long RowCount { get; set; }

        /// <summary>
        /// Byte count
        /// </summary>
        [JsonPropertyName("byte_count")]
        public long ByteCount { get; set; }
    }

    /// <summary>
    /// Result data from statement execution
    /// </summary>
    public class DatabricksSqlResult
    {
        /// <summary>
        /// Data rows (when disposition is INLINE)
        /// </summary>
        [JsonPropertyName("data_array")]
        public List<List<object?>>? DataArray { get; set; }

        /// <summary>
        /// External links (when disposition is EXTERNAL_LINKS)
        /// </summary>
        [JsonPropertyName("external_links")]
        public List<DatabricksSqlExternalLink>? ExternalLinks { get; set; }

        /// <summary>
        /// Link to next chunk (for pagination)
        /// </summary>
        [JsonPropertyName("next_chunk_internal_link")]
        public string? NextChunkInternalLink { get; set; }
    }

    /// <summary>
    /// External link for large result sets
    /// </summary>
    public class DatabricksSqlExternalLink
    {
        /// <summary>
        /// Chunk index
        /// </summary>
        [JsonPropertyName("chunk_index")]
        public int ChunkIndex { get; set; }

        /// <summary>
        /// External URL to download chunk
        /// </summary>
        [JsonPropertyName("external_link")]
        public string ExternalLink { get; set; } = null!;

        /// <summary>
        /// Expiration time
        /// </summary>
        [JsonPropertyName("expiration")]
        public DateTimeOffset? Expiration { get; set; }
    }

    /// <summary>
    /// DTO for Gupy application data from gupy_aplicacoes table
    /// </summary>
    public class GupyAplicacaoDto
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
        public DateTimeOffset? DataCriacao { get; set; }
        public DateTimeOffset? DataAtualizacao { get; set; }
        public string? IdTrabalho { get; set; }
        public string? IdPassoAtual { get; set; }
        public string? IdCandidato { get; set; }
        public int? Afinidade { get; set; }
        public string? Fonte { get; set; }
        public string? StatusPasso { get; set; }
        public DateTimeOffset? PassoAtualData { get; set; }
        public string? PassoAtualNome { get; set; }
        public string? NomeTrabalho { get; set; }
        public string? StatusTrabalho { get; set; }
        public bool? Desqualificado { get; set; }
        public string? DesqualificacaoRazao { get; set; }
        public DateTimeOffset? DataExtracao { get; set; }
    }

    /// <summary>
    /// DTO for Gupy candidate data from gupy_candidatos table
    /// </summary>
    public class GupyCandidatoDto
    {
        public string? Id { get; set; }
        public string? PrimeiroNome { get; set; }
        public string? SegundoNome { get; set; }
        public string? Email { get; set; }
        public DateTimeOffset? Nascimento { get; set; }
        public string? Telefone { get; set; }
        public string? Genero { get; set; }
        public string? Raca { get; set; }
        public string? EstadoCivil { get; set; }
        public string? Cidade { get; set; }
        public string? CodigoPais { get; set; }
        public string? Cep { get; set; }
        public DateTimeOffset? Criacao { get; set; }
        public DateTimeOffset? Atualizacao { get; set; }
        public string? GrauEducacao { get; set; }
        public string? InstituicaoEducacao { get; set; }
        public string? CursoEducacao { get; set; }
        public string? StatusEducacao { get; set; }
        public string? CargoExperiencia { get; set; }
        public string? OrganizacaoExperiencia { get; set; }
        public string? AtividadesDesempenhadas { get; set; }
        public DateTimeOffset? DataExtracao { get; set; }
    }

    /// <summary>
    /// Response DTO for Gupy sync operation
    /// </summary>
    public class GupySyncResponseDto
    {
        public int TotalApplications { get; set; }
        public int CandidatesFound { get; set; }
        public int CandidatesCreated { get; set; }
        public int CandidatesUpdated { get; set; }
        public int JobsCreated { get; set; }
        public int JobsUpdated { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTimeOffset ProcessedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}