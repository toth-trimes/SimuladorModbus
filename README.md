# Simulador Modbus RTU - C# (.NET Windows Forms)

Este projeto é um **simulador de multi-dispositivos Modbus RTU via porta serial (COM)** desenvolvido em **C#**, com foco em auxiliar desenvolvedores, testadores e integradores de sistemas que trabalham com automação industrial, controle e leitura de sensores.

---

## 🧩 Funcionalidades

- Criação de **múltiplos dispositivos Modbus** (Slaves), cada um com seu próprio:
  - Endereço (ID)
  - Mapa de registradores (`Holding Registers`)
  - Entradas e saídas digitais

- Comunicação via **porta serial COM (virtual ou física)**
- Suporte aos seguintes **códigos de função Modbus**:
  - `0x03` – Leitura de Holding Registers
  - `0x06` – Escrita de um único registrador
  - `0x10` – Escrita de múltiplos registradores
  - `0x01` – Leitura de saídas digitais (coils)
  - `0x02` – Leitura de entradas digitais
  - `0x05` – Escrita de uma saída digital

- Os dispositivos e seus dados são carregados a partir de arquivos JSON na pasta `./dados/`.
- As alterações feitas durante a simulação podem ser **salvas novamente nos arquivos**.

---

## 💡 Propósito

O objetivo deste simulador é **substituir dispositivos físicos** em bancadas de testes, sistemas de P&D ou simulações industriais.
Facilitar o desenvolvimento de softwares de controle, simulando a resposta dos dispositivos.

---

## 📦 Estrutura do Projeto

```
SimuladorModbus/
├── Principal.cs              # Interface principal e gerenciador de dispositivos
├── ModbusSlave.cs           # Lógica do protocolo Modbus para cada dispositivo
├── DescricaoRegistradores.cs # Descrição textual dos registradores (ex: TLB4)
├── dados/                   # Arquivos .json para configuração de cada dispositivo
│   ├── TLB4.json
│   ├── STR6.json
│   └── ...
├── README.md
├── LICENSE (opcional)
└── SimuladorModbus.csproj
```

---

## ▶️ Como Iniciar

1. **Requisitos**:
   - Windows
   - Visual Studio (2022 ou superior)
   - .NET Framework 4.7.2 ou superior (Windows Forms)
   - [com0com - Null-modem emulator](https://com0com.sourceforge.net/)
     - Instale o `com0com` para criar pares de portas COM virtuais. Ex: `COM3 <-> COM4`
     - Configure o terminal Modbus Master (como Modbus Poll) para conectar na outra ponta do par

2. **Clone o repositório**:

```bash
git clone https://github.com/seuusuario/simulador-modbus.git
```

3. **Abra o projeto no Visual Studio**

4. Compile e execute

---

## ⚙️ Como Usar

1. **Execute o programa**
2. Selecione a porta COM desejada (ex: COM3, COM4, etc)
   - Você pode usar softwares como **com0com** para criar portas COM virtuais pareadas.
3. Selecione a taxa de baud (ex: 9600, 19200)
4. Clique em **Conectar**
5. O simulador aguarda requisições Modbus RTU nessa porta.
6. Se você estiver utilizando o com0com, basta conectar o seu programa na porta pareada, e enviar os comandos. Este simulador responderá através dessa porta e o seu programa receberá de volta a resposta.
7. Ao sair do programa, será perguntado se deseja salvar os dados atualizados. Caso escolha sim, os dados alterados por registradores de escrita serão salvos no arquivo dados já existententes. Ou seja a próxima vez que abrir o programa estarão nesse novo formato.

---

## 📁 Arquivos de Dispositivos

Cada dispositivo Modbus é descrito por um arquivo JSON dentro da pasta `/dados`:

Exemplo de `dispositivo1.json`:
```json
{
  "ID_Slave": 3,
  "Registros": [0, 3, 1, 1, 1, 0, 0, 1, 1, 0, 1000, 2000, 3000, 0, 0, 0, 0, 0, 0, 0, 1234, 5678, ...],
  "Entradas": [0, 0],
  "Saidas": [0, 0, 0]
}
```
Você pode adicionar quantos dispositivos quiser, basta criar novos arquivos JSON com nomes diferentes.
Ao iniciar o programa ele abrirá todos os arquivos dessa pasta.
Ao conectar todos os dispositivos estarão disponíveis para leitura.

---

## 🔐 Licença

Este projeto é open source:
- [GPLv3](https://www.gnu.org/licenses/gpl-3.0.html) — exige que derivados permaneçam abertos

---

## 🙋 Suporte e Contribuições

Sinta-se à vontade para:
- Criar *issues* com bugs ou sugestões
- Abrir *pull requests* com melhorias ou novos dispositivos
- Compartilhar com colegas e contribuir com a comunidade de automação

---

**Feito por:** Fábio Rodrigues
**Contato:** fabioluiz.toth@gmail.com
