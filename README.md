# Itemiza - ASP.NET Core MVC + SQL Server

Projeto inicial funcional para envio e armazenamento de notas fiscais e cadastro manual de compras.

## Requisitos
- Visual Studio 2022 com a carga de trabalho **ASP.NET e desenvolvimento web**
- .NET 8 SDK
- SQL Server LocalDB (normalmente instalado com o Visual Studio) ou SQL Server

## Abrir e executar
1. Extraia o ZIP.
2. Abra `Itemiza.csproj` no Visual Studio.
3. Confira a connection string em `appsettings.json`. O padrão usa `(localdb)\\MSSQLLocalDB`.
4. Abra **Tools > NuGet Package Manager > Package Manager Console**.
5. Execute:
   ```powershell
   Add-Migration InitialCreate
   Update-Database
   ```
   Se os comandos não forem reconhecidos, confirme que `Microsoft.EntityFrameworkCore.Tools` foi restaurado.
6. Pressione F5. A tela inicial abre na página Escanear Nota Fiscal.

## Funcionalidades incluídas
- Interface responsiva inspirada na referência enviada.
- Upload de PDF/JPG/JPEG/PNG com limite de 10 MB.
- Validação de extensão e tamanho no servidor.
- Arquivo armazenado em `wwwroot/uploads` com nome aleatório gerado pelo servidor.
- Metadados do arquivo salvos em `NotasFiscais` no SQL Server.
- Lista de notas enviadas e opção de abrir o arquivo.
- Cadastro manual de compra e vínculo opcional com uma nota fiscal.
- Contadores de notas, compras e total de compras.

## Observações importantes
- O reconhecimento OCR/IA ainda não está conectado; a interface apenas apresenta as etapas planejadas.
- Para produção, não é recomendável guardar documentos privados em `wwwroot`, pois essa pasta pode ser servida publicamente. Use armazenamento privado fora de `wwwroot` e um endpoint autorizado para baixar os arquivos.
- O tipo MIME informado pelo navegador não é prova de que o arquivo é seguro. Para produção, valide assinatura real dos arquivos, adicione autenticação/autorização, antivírus, limites de requisição e políticas de retenção.
- A página de usuário é apenas visual nesta versão; autenticação ainda não foi implementada.
