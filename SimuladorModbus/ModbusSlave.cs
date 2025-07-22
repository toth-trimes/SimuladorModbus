using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace SimuladorModbus
{
    public class DispositivoData
    {
        public byte ID_Slave { get; set; }
        public ushort[] Registros { get; set; }
        public bool[] Entradas { get; set; }
        public bool[] Saidas { get; set; }
    }

    public class ModbusSlave
    {
        public byte Endereco { get; private set; }
        public ushort[] Registros { get; private set; } = new ushort[100];
        public bool[] Entradas { get; private set; } = new bool[64];
        public bool[] Saidas { get; private set; } = new bool[64];

        private List<byte> bufferRecebido = new List<byte>();

        public ModbusSlave(byte endereco)
        {
            Endereco = endereco;
        }

        public byte[] ProcessarBuffer(byte[] dados)
        {
            bufferRecebido.AddRange(dados);

            while (bufferRecebido.Count >= 8)
            {
                if (bufferRecebido[0] != Endereco)
                {
                    bufferRecebido.RemoveAt(0);
                    continue;
                }

                byte funcao = bufferRecebido[1];
                int tamanhoEsperado = 8; // Mínimo comum para 0x03, 0x06, 0x10, 0x02, 0x05

                if (funcao == 0x10)
                {
                    if (bufferRecebido.Count < 7)
                        return null;
                    tamanhoEsperado = 9 + bufferRecebido[6];
                }

                if (bufferRecebido.Count < tamanhoEsperado)
                    return null;

                byte[] pacote = bufferRecebido.GetRange(0, tamanhoEsperado).ToArray();
                bufferRecebido.RemoveRange(0, tamanhoEsperado);

                ushort crcCalculado = CalcularCRC(pacote, tamanhoEsperado - 2);
                ushort crcRecebido = (ushort)(pacote[tamanhoEsperado - 2] | (pacote[tamanhoEsperado - 1] << 8));

                if (crcCalculado != crcRecebido)
                    return null;

                return ProcessarPacoteModbus(pacote);
            }
            return null;
        }

        private byte[] ProcessarPacoteModbus(byte[] pacote)
        {
            byte funcao = pacote[1];
            switch (funcao)
            {
                case 0x03: return ProcessarFuncao03(pacote);
                case 0x06: return ProcessarFuncao06(pacote);
                case 0x10: return ProcessarFuncao16(pacote);
                case 0x02: return ProcessarFuncao02(pacote);
                case 0x05: return ProcessarFuncao05(pacote);
                default: return null;
            }
        }

        /// <summary>
        /// Processa o comando Modbus RTU função 0x03 (Read Holding Registers).
        /// Essa função retorna os valores de registradores de 16 bits.
        /// 
        /// Estrutura da requisição:
        /// [Endereço][0x03][Addr Hi][Addr Lo][Qtd Hi][Qtd Lo][CRC Lo][CRC Hi]
        /// 
        /// Estrutura da resposta:
        /// [Endereço][0x03][Qtd de bytes][Dado1 Hi][Dado1 Lo]...[CRC Lo][CRC Hi]
        /// 
        /// Exemplo: Requisitar 2 registros a partir do endereço 0x0010
        /// → Mestre envia: 01 03 00 10 00 02 CRC
        /// ← Escravo responde: 01 03 04 00 2A 00 3F CRC
        /// </summary>
        private byte[] ProcessarFuncao03(byte[] pacote)
        {
            ushort addr = (ushort)((pacote[2] << 8) | pacote[3]);
            ushort quantidade = (ushort)((pacote[4] << 8) | pacote[5]);

            if (addr + quantidade > Registros.Length)
                return null;

            byte[] resposta = new byte[3 + 2 * quantidade + 2]; // endereço + função + byteCount + dados + CRC

            resposta[0] = Endereco;
            resposta[1] = 0x03;
            resposta[2] = (byte)(quantidade * 2); // número de bytes de dados

            for (int i = 0; i < quantidade; i++)
            {
                ushort valor = Registros[addr + i];
                resposta[3 + i * 2] = (byte)(valor >> 8);     // High
                resposta[4 + i * 2] = (byte)(valor & 0xFF);   // Low
            }

            ushort crc = CalcularCRC(resposta, resposta.Length - 2);
            resposta[resposta.Length - 2] = (byte)(crc & 0xFF);
            resposta[resposta.Length - 1] = (byte)(crc >> 8);
            return resposta;
        }

        /// <summary>
        /// Processa o comando Modbus RTU função 0x06 (Write Single Register).
        /// Escreve um valor de 16 bits em um registrador específico.
        /// 
        /// Estrutura da requisição:
        /// [Endereço][0x06][Addr Hi][Addr Lo][Valor Hi][Valor Lo][CRC Lo][CRC Hi]
        /// 
        /// Estrutura da resposta:
        /// → Eco completo da requisição (o mesmo pacote enviado pelo mestre)
        /// 
        /// Exemplo: Escrever o valor 0x002A no endereço 0x0010
        /// → Mestre envia: 01 06 00 10 00 2A CRC
        /// ← Escravo responde: 01 06 00 10 00 2A CRC
        /// </summary>
        private byte[] ProcessarFuncao06(byte[] pacote)
        {
            ushort addr = (ushort)((pacote[2] << 8) | pacote[3]);
            ushort valor = (ushort)((pacote[4] << 8) | pacote[5]);

            if (addr >= Registros.Length)
                return null;

            Registros[addr] = valor;

            // Retorna o mesmo pacote recebido (eco)
            byte[] resposta = new byte[8];
            Array.Copy(pacote, 0, resposta, 0, 6);
            ushort crc = CalcularCRC(resposta, 6);
            resposta[6] = (byte)(crc & 0xFF);
            resposta[7] = (byte)(crc >> 8);
            return resposta;
        }

        /// <summary>
        /// Processa o comando Modbus RTU função 0x10 (Write Multiple Registers).
        /// Escreve múltiplos valores de 16 bits em sequência nos registradores.
        /// 
        /// Estrutura da requisição:
        /// [Endereço][0x10][Addr Hi][Addr Lo][Qtd Hi][Qtd Lo][Qtd Bytes][Dado1 Hi][Dado1 Lo]...[CRC Lo][CRC Hi]
        /// 
        /// Estrutura da resposta:
        /// [Endereço][0x10][Addr Hi][Addr Lo][Qtd Hi][Qtd Lo][CRC Lo][CRC Hi]
        /// 
        /// Exemplo: Escrever 2 valores (0x000A, 0x0014) nos registradores a partir de 0x0010
        /// → Mestre envia: 01 10 00 10 00 02 04 00 0A 00 14 CRC
        /// ← Escravo responde: 01 10 00 10 00 02 CRC
        /// </summary>
        private byte[] ProcessarFuncao16(byte[] pacote)
        {
            ushort addr = (ushort)((pacote[2] << 8) | pacote[3]);
            ushort quantidade = (ushort)((pacote[4] << 8) | pacote[5]);
            byte byteCount = pacote[6];

            if (addr + quantidade > Registros.Length || byteCount != quantidade * 2)
                return null;

            for (int i = 0; i < quantidade; i++)
            {
                ushort valor = (ushort)((pacote[7 + i * 2] << 8) | pacote[8 + i * 2]);
                Registros[addr + i] = valor;
            }

            byte[] resposta = new byte[8];
            resposta[0] = Endereco;
            resposta[1] = 0x10;
            resposta[2] = pacote[2]; // Addr High
            resposta[3] = pacote[3]; // Addr Low
            resposta[4] = pacote[4]; // Qtd High
            resposta[5] = pacote[5]; // Qtd Low
            ushort crc = CalcularCRC(resposta, 6);
            resposta[6] = (byte)(crc & 0xFF);
            resposta[7] = (byte)(crc >> 8);
            return resposta;
        }

        /// <summary>
        /// Processa o comando Modbus RTU função 0x02 (Read Discrete Inputs).
        /// Lê o estado (true/false) de entradas digitais somente leitura (1 bit cada).
        /// 
        /// Estrutura da requisição:
        /// [Endereço][0x02][Addr Hi][Addr Lo][Qtd Hi][Qtd Lo][CRC Lo][CRC Hi]
        /// 
        /// Estrutura da resposta:
        /// [Endereço][0x02][Qtd de bytes][Bits em grupos de 8][CRC Lo][CRC Hi]
        /// Cada bit representa uma entrada digital (0 = desligado, 1 = ligado)
        /// 
        /// Exemplo: Ler 10 entradas a partir do endereço 0x0000
        /// → Mestre envia: 01 02 00 00 00 0A CRC
        /// ← Escravo responde: 01 02 02 5A 00 CRC
        /// </summary>
        private byte[] ProcessarFuncao02(byte[] pacote)
        {
            ushort addr = (ushort)((pacote[2] << 8) | pacote[3]);
            ushort quantidade = (ushort)((pacote[4] << 8) | pacote[5]);

            if (addr + quantidade > Entradas.Length)
                return null;

            int numBytes = (quantidade + 7) / 8;
            byte[] resposta = new byte[3 + numBytes + 2]; // endereço + função + byte count + dados + CRC

            resposta[0] = Endereco;
            resposta[1] = 0x02;
            resposta[2] = (byte)numBytes;

            for (int i = 0; i < quantidade; i++)
            {
                if (Entradas[addr + i])
                    resposta[3 + (i / 8)] |= (byte)(1 << (i % 8));
            }

            ushort crc = CalcularCRC(resposta, resposta.Length - 2);
            resposta[resposta.Length - 2] = (byte)(crc & 0xFF);
            resposta[resposta.Length - 1] = (byte)(crc >> 8);
            return resposta;
        }

        /// <summary>
        /// Processa o comando Modbus RTU função 0x05 (Write Single Coil).
        /// Define o estado de uma única saída digital (coil) como ligada (0xFF00) ou desligada (0x0000).
        /// 
        /// Estrutura da requisição:
        /// [Endereço][0x05][Addr Hi][Addr Lo][0xFF|0x00][0x00][CRC Lo][CRC Hi]
        /// 
        /// Estrutura da resposta:
        /// → Eco completo da requisição
        /// 
        /// Exemplo: Ligar a saída coil no endereço 0x0002
        /// → Mestre envia: 01 05 00 02 FF 00 CRC
        /// ← Escravo responde: 01 05 00 02 FF 00 CRC
        /// </summary>
        private byte[] ProcessarFuncao05(byte[] pacote)
        {
            ushort addr = (ushort)((pacote[2] << 8) | pacote[3]);
            ushort valor = (ushort)((pacote[4] << 8) | pacote[5]);

            if (addr >= Saidas.Length)
                return null;

            if (valor == 0xFF00)
                Saidas[addr] = true;
            else if (valor == 0x0000)
                Saidas[addr] = false;
            else
                return null;

            // Eco da requisição
            byte[] resposta = new byte[8];
            Array.Copy(pacote, 0, resposta, 0, 6);
            ushort crc = CalcularCRC(resposta, 6);
            resposta[6] = (byte)(crc & 0xFF);
            resposta[7] = (byte)(crc >> 8);
            return resposta;
        }

        private ushort CalcularCRC(byte[] dados, int tamanho)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < tamanho; i++)
            {
                crc ^= dados[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return crc;
        }
    }
}
