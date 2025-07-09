# To learn more about how to use Nix to configure your environment
# see: https://developers.google.com/idx/guides/customize-idx-env
{ pkgs, ... }: {
  channel = "stable-24.05";

  packages = [
    pkgs.dotnet-sdk_8
    pkgs.dotnet-aspnetcore_8
  ];

  env = {};

  idx = {
    extensions = [
      "ms-dotnettools.vscode-dotnet-runtime"
      "muhammad-sammy.csharp"
    ];

    previews = {
      enable = true;
      previews = {
        # Puedes habilitar el servidor ASP.NET aquí si deseas
        # web = {
        #   command = ["dotnet" "run"];
        #   manager = "web";
        #   env = { PORT = "$PORT"; };
        # };
      };
    };

    workspace = {
      onCreate = {
        default.openFiles = [ ".idx/dev.nix" "README.md" ];
      };
      onStart = {
        # Por ejemplo:
        # build = "dotnet build";
      };
    };
  };
}