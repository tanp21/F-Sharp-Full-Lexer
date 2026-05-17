{
  description = "Development shell for the Python FsLex clone and official F# lexer parity checks";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs =
    {
      self,
      nixpkgs,
      flake-utils,
    }:
    flake-utils.lib.eachDefaultSystem (
      system:
      let
        pkgs = import nixpkgs {
          inherit system;
          config = {
            allowUnfree = true;
            permittedInsecurePackages = [
              # FsLexYacc/global.json pins the 6.0 SDK line. Keep this scoped to the dev shell.
              "dotnet-sdk-6.0.428"
            ];
          };
        };

        dotnet = pkgs.dotnetCorePackages.combinePackages [
          pkgs.dotnetCorePackages.sdk_10_0
          pkgs.dotnetCorePackages.sdk_8_0
          pkgs.dotnetCorePackages.sdk_6_0
        ];
      in
      {
        devShells.default = pkgs.mkShell {
          packages = [
            dotnet
            pkgs.direnv
            pkgs.git
            pkgs.jq
            pkgs.nix-direnv
            pkgs.python313
            pkgs.uv
          ];

          DOTNET_ROOT = "${dotnet}/share/dotnet";
          DOTNET_MULTILEVEL_LOOKUP = "0";
          DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1";
          DOTNET_CLI_TELEMETRY_OPTOUT = "1";
          DOTNET_NOLOGO = "1";
          DOTNET_ROLL_FORWARD = "Major";

          shellHook = ''
            export NUGET_PACKAGES="$PWD/.nix-cache/nuget"
            export UV_CACHE_DIR="$PWD/.nix-cache/uv"

            mkdir -p "$NUGET_PACKAGES" "$UV_CACHE_DIR"

            export PATH="$PWD/.venv/bin:$PATH"

            cat <<EOF
F# lexer development shell

Python checks:
  uv run pytest
  uv run ruff check .
  uv run fsharp-lexer-generate --check
  uv run fsharp-lexer-diff tests/fixtures/core_module.fs

Official F#/FsLexYacc checks:
  dotnet --info
  dotnet publish fsharp/proto.proj /restore /p:Configuration=Proto /p:DotNetBuild=false /p:DotNetBuildSourceOnly=false /p:IgnoreMibc=true /p:NoOptimizationData=true
  dotnet run -c Proto --project tools/OfficialTokenizer -- tests/fixtures/core_module.fs
  dotnet build FsLexYacc/FsLexYacc.sln
  dotnet build fsharp/FSharp.Compiler.Service.slnx

Caches:
  NUGET_PACKAGES=$NUGET_PACKAGES
  UV_CACHE_DIR=$UV_CACHE_DIR
EOF
          '';
        };
      }
    );
}
