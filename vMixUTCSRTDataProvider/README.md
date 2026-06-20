# SRT Monitor — Data Provider para vMixUTC

Widget de monitoramento de streams SRT integrado ao [vMixUTC](https://github.com/elgarf/vMixUTC), desenvolvido como um **Data Provider** externo.

---

## O que é

O **SRT Monitor** permite receber e visualizar streams de vídeo via protocolo **SRT (Secure Reliable Transport)** diretamente dentro do vMixUTC, sem precisar abrir um player externo. Ideal para monitorar sinais de entrada antes de colocá-los no ar no vMix.

---

## Funcionalidades

| Recurso | Descrição |
|---|---|
| Modo Listener | Aguarda conexão de entrada na porta configurada |
| Modo Caller | Conecta ativamente em um host remoto (IP + Porta) |
| Preview de vídeo | Janela de preview embutida no widget com redimensionamento vertical |
| Controle de áudio | Checkbox por instância — isola completamente o áudio de cada widget |
| IP padrão automático | Pré-preenche o IP do host vMix já configurado no UTC |
| Configurações avançadas | Latência, AES passphrase, key length (128/192/256 bits), Stream ID |
| Ícones ▶ / ■ | Botões conectar/desconectar com ícones Font Awesome |
| Múltiplas instâncias | Cada widget opera de forma completamente independente |

---

## Download e Instalação

### Download pronto para usar

**[⬇ Baixar SrtMonitorDataProvider.zip](https://github.com/rafalau/vMixUTC/releases/download/v1.0.0-srt/SrtMonitorDataProvider.zip)** (~44 MB)

O ZIP já inclui o plugin **e** as DLLs nativas do VLC. Não é necessário instalar o VLC separadamente.

### Como instalar

1. Extraia o ZIP
2. Copie a pasta `DataProviders` extraída para dentro da pasta de instalação do **vMixUTC**, mesclando com a pasta existente:

```
vMixUTC/
└── DataProviders/           ← cole aqui o conteúdo extraído
    ├── SrtMonitorDataProvider.dll
    └── libvlc/
        └── win-x64/
            ├── libvlc.dll
            ├── libvlccore.dll
            └── plugins/
```

3. Reinicie o vMixUTC — o widget **SRT Monitor** aparecerá na lista de Data Providers.

---

## Tecnologias utilizadas

| Biblioteca | Versão | Finalidade |
|---|---|---|
| [LibVLCSharp](https://github.com/videolan/libvlcsharp) | 3.8.5 | Wrapper .NET para libvlc |
| [LibVLCSharp.WPF](https://github.com/videolan/libvlcsharp) | 3.8.5 | Controle WPF para preview de vídeo |
| libvlc (VLC) | 3.x | Engine de reprodução / SRT decoder |
| .NET Framework | 4.7.2 | Target framework do vMixUTC |
| Costura.Fody | — | Embute as DLLs gerenciadas no assembly final |

---

## Arquitetura

O plugin implementa a interface `IvMixDataProvider` do vMixUTC:

```
vMixControlExternalData  (host widget no UTC)
    └── SRTDataProvider  (IvMixDataProvider)
            ├── LibVLC   (instância por widget — isolamento de áudio)
            └── OnWidgetUI  (UI WPF com VideoView embutido)
                    └── MediaPlayer  (por instância)
```

Cada widget cria sua própria instância de `LibVLC` e `MediaPlayer`, garantindo isolamento total de vídeo e áudio entre múltiplos monitores na mesma tela.

O controle de áudio usa a opção `:no-audio` diretamente na mídia quando o checkbox é desmarcado, e reconecta automaticamente ao alternar — evitando qualquer compartilhamento de estado no pipeline de áudio do Windows.

---

## Como compilar

```bash
# Restaurar pacotes NuGet e compilar (gera o DLL em vMixController/bin/Debug/DataProviders/)
MSBuild vMixUTCSRTDataProvider.csproj /p:Configuration=Debug /p:Platform=AnyCPU /t:Build
```

---

## Créditos

Idealizado por **[rafalau](https://github.com/rafalau)**.  
Implementado com assistência de **Claude Sonnet 4.6** (Anthropic).
