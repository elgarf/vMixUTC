# SRT Monitor — Data Provider para vMixUTC

Widget de monitoramento de streams SRT integrado ao [vMixUTC](https://github.com/elgarf/vMixUTC), desenvolvido como um **Data Provider** externo.

---

## Download e Instalação

**[⬇ Baixar SrtMonitorDataProvider.zip](https://github.com/rafalau/vMixUTC/releases/download/v1.1.0-srt/SrtMonitorDataProvider.zip)** (~86 MB — já inclui VLC)

1. Extraia o ZIP
2. Copie a pasta `DataProviders` para dentro da pasta de instalação do **vMixUTC**, mesclando com a pasta existente
3. Reinicie o vMixUTC — o widget **SRT Monitor** aparecerá na lista de Data Providers

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

## Enable Auto Sync *(v1.1)*

O vMixUTC não atualiza automaticamente quando algo muda no vMix — é necessário clicar em **Sync** manualmente. A partir da v1.1, o menu do UTC conta com o toggle **Enable Auto Sync**.

Quando ativado, o UTC consulta o vMix a cada segundo e detecta qualquer alteração — inputs adicionados, textos modificados, configurações trocadas — atualizando a interface automaticamente.

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

## Créditos

Idealizado por **[rafalau](https://github.com/rafalau)**.  
Implementado com assistência de **Claude Sonnet 4.6** (Anthropic).
