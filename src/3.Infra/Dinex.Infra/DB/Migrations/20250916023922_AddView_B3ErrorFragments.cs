using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dinex.Infra.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddView_B3ErrorFragments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // opcional: índice que ajuda no filtro/ordenação por ImportJobId + RowNumber
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ix_b3rows_job_rownum
                ON public.""B3StatementRows"" (""ImportJobId"", ""RowNumber"");
                ");

            // view que 'explode' o campo Error em linhas, separando por pipe
            migrationBuilder.Sql(@"
                CREATE OR REPLACE VIEW public.""v_b3_error_fragments"" AS
                SELECT
                  r.""Id""                AS ""RowId"",
                  r.""ImportJobId"",
                  r.""RowNumber"",
                  r.""RawLineJson"",
                  r.""CreatedAt"",
                  frag.idx               AS ""ErrorIndex"",
                  trim(both ' ' from frag.msg) AS ""Error""
                FROM public.""B3StatementRows"" r
                CROSS JOIN LATERAL (
                  SELECT msg, idx
                  FROM regexp_split_to_table(coalesce(r.""Error"", ''), '\s*\|\s*') WITH ORDINALITY AS t(msg, idx)
                ) AS frag
                WHERE r.""DeletedAt"" IS NULL
                  AND (r.""Error"" IS NOT NULL OR r.""Status"" = 'Erro');
                ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS public.""v_b3_error_fragments"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS public.ix_b3rows_job_rownum;");
        }

    }
}
