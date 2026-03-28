namespace DbMercado.Domain.Shared;

/// <summary>
/// Classe responsável por armazenar as configurações de valores fixos da aplicação.
/// As configurações com valores que poder ser altrerados em tempo real e que serão automaticamente
/// espelhadas na aplicação são armazenadas na tabela de Parametro.
/// </summary>
public class ApplicationSettings
{

    #region Aplicação

    /// <summary>
    /// Encapsula parâmetros fixos da aplicação, como nome, ambiente, cultura etc.
    /// </summary>
    public static class Application
    {
        public const string ApplicationName = "DbMercado.API";
        public const string ApplicationPrefix = "DbMercadoApi";
        public const string AnonymousUser = "anonymous";
        public const string DefaultCulture = "pt-BR";
        
        public enum AppEnvironments
        {
            Desenvolvimento,
            Simulacao,
            Producao
        }

        /// <summary>
        /// Responsável por informar em qual o ambiente a aplicação está executando:
        /// Desenvolvimento, Simulação ou Produção.
        /// </summary>
        public static int Ambiente
        {
            get
            {
                return ((int)AppEnvironments.Desenvolvimento);
            }
        }
    }

    #endregion Aplicação




    #region Permissions

    public static class Permissions
    {
        public static class Modulo
        {
            public const string Access = "modulo.access";
        }

        //public static class Pedido
        //{
        //    public const string Access = "pedido.access";
        //    public const string Approve = "pedido.approve";
        //}

        public static class Usuario
        {
            public const string Manage = "usuario.manage";
        }
    }

    #endregion Permissions




    #region Database

    /// <summary>
    /// Encapsula parâmetros relacionados à conexão com o banco de dados.   
    /// </summary>
    public static class DataBase
    {
        private static string? _connectionString;

        /// <summary>
        /// Define a string de conexão antes do uso do contexto EF sem opções injetadas (ex.: logging).
        /// Normalmente chamado na inicialização da API a partir de <c>Configuration</c> ou variáveis de ambiente.
        /// </summary>
        public static void SetConnectionString(string? connectionString) =>
            _connectionString = connectionString;

        /// <summary>
        /// String de conexão configurada em tempo de execução (não armazene credenciais no repositório).
        /// </summary>
        public static string ConnectionString =>
            _connectionString
            ?? throw new InvalidOperationException(
                "Connection string não configurada. Chame ApplicationSettings.DataBase.SetConnectionString na inicialização ou configure ConnectionStrings:DefaultConnection.");
    }

    #endregion Database




    #region Cache

    /// <summary>
    /// Encapsdula parâmetros relacionados ao cache da aplicação.
    /// </summary>
    public static class Cache
    {
        /// <summary>
        /// Chave padronizadas para acesso ao cache.
        /// </summary>
        /// 
        public static class CacheKeys
        {
            public const string Mensagem = "AppMensagem";
            public const string MensagemToast = "MensagemToast";
            public const string ReturnUrl = "UserSessionReturnUrl";
        }

        /// <summary>
        /// O cache type define o escopo de vida útil da entrada no cache, e está diretamente
        /// ligado aos velores de AbsoluteExpiration e SlideExpiration. 
        /// </summary>
        public enum CacheType
        {
            Singleton,
            Scope,
            Transient
        }

        public static class CacheExpiration
        {
            public static int SingletonAbsoluteExpiration = 31536000;
            public static int ScopeAbsoluteExpiration = 1800;
            public static int TransientAbsoluteExpiration = 10;
            public static int SingletonSlideExpiration = 15552000;
            public static int ScopeSlideExpiration = 1200;
            public static int TransientSlideExpiration = 5;
        }
    }


    #endregion Cache




    #region Claim

    public static class ClaimApp
    {

        public enum NivelAcesso
        {
            Root = 1000,
            Administrador = 100,
            Gestor = 90,
            Analista = 80,
            Estagiario = 10
        }

        public enum ClaimKeys
        {
            NivelAcesso,
            Modulo
        }
    }


    #endregion Claim
}
