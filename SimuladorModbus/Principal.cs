using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimuladorModbus
{

    public partial class Principal : Form
    {
        private Dictionary<string, ModbusSlave> dispositivos = new Dictionary<string, ModbusSlave>();
        private SerialPort portaSerial;

        public Principal()
        {
            InitializeComponent();
            this.Load += Form1_Load; // Garante que o método seja chamado ao iniciar o formulário
            //this.FormClosing += Principal_FormClosing;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            criarECarregarDispositivo();

            // Carrega as portas COM disponíveis
            comboBox_PortaCOM.Items.Clear();
            var portas = SerialPort.GetPortNames();
            Array.Sort(portas); // opcional: ordena COM1, COM2, ...
            comboBox_PortaCOM.Items.AddRange(portas);
            comboBox_PortaCOM.Items.Add("Atualizar");


            if (comboBox_PortaCOM.Items.Count > 0)
                comboBox_PortaCOM.SelectedIndex = 0;

            // Carrega opções de BaudRate
            comboBox_BaudRate.Items.AddRange(new object[] { "9600", "19200", "38400", "57600", "115200" });
            comboBox_BaudRate.SelectedIndex = 0;
        }

        private void criarECarregarDispositivo()
        {
            string pastaDados = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dados");

            if (!Directory.Exists(pastaDados))
            {
                MessageBox.Show("Pasta 'dados' não encontrada.");
                return;
            }

            string[] arquivosJson = Directory.GetFiles(pastaDados, "*.json");

            foreach (string caminhoArquivo in arquivosJson)
            {
                string nomeDispositivo = Path.GetFileNameWithoutExtension(caminhoArquivo);

                try
                {
                    string conteudo = File.ReadAllText(caminhoArquivo);
                    var dados = JsonSerializer.Deserialize<DispositivoData>(conteudo);

                    if (dados != null)
                    {
                        var slave = new ModbusSlave(dados.ID_Slave);

                        // Registros
                        if (dados.Registros != null)
                        {
                            for (int i = 0; i < Math.Min(dados.Registros.Length, slave.Registros.Length); i++)
                                slave.Registros[i] = dados.Registros[i];
                        }

                        // Entradas
                        if (dados.Entradas != null)
                        {
                            for (int i = 0; i < Math.Min(dados.Entradas.Length, slave.Entradas.Length); i++)
                                slave.Entradas[i] = dados.Entradas[i];
                        }

                        // Saídas
                        if (dados.Saidas != null)
                        {
                            for (int i = 0; i < Math.Min(dados.Saidas.Length, slave.Saidas.Length); i++)
                                slave.Saidas[i] = dados.Saidas[i];
                        }

                        dispositivos[nomeDispositivo] = slave;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao carregar dispositivo '{nomeDispositivo}': {ex.Message}");
                }
            }
        }


        private void button_Conectar_Click(object sender, EventArgs e)
        {
            if (portaSerial == null)
            {
                string porta = comboBox_PortaCOM.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(porta))
                {
                    textBox_StatusCOM.Text = "Selecione uma porta COM.";
                    return;
                }

                if (!int.TryParse(comboBox_BaudRate.SelectedItem?.ToString(), out int baudRate))
                {
                    textBox_StatusCOM.Text = "BaudRate inválido.";
                    return;
                }

                try
                {
                    portaSerial = new SerialPort(porta, baudRate, Parity.None, 8, StopBits.One);
                    portaSerial.Open();
                    portaSerial.DataReceived += PortaSerial_DataReceived;

                    textBox_StatusCOM.Text = $"Porta {porta} aberta com sucesso.";
                    button_CriarCOM.Text = "Desconecta";
                    comboBox_PortaCOM.Enabled = false;
                    comboBox_BaudRate.Enabled = false; 

                }
                catch (Exception ex)
                {
                    textBox_StatusCOM.Text = $"Erro ao abrir porta: {ex.Message}";
                }
            }
            else //desconecta
            {
                var resultado = MessageBox.Show("Deseja salvar os dados dos dispositivos?", "Salvar Dados", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resultado == DialogResult.Yes)
                {
                    SalvarTodosDispositivos();
                }

                portaSerial.Close();
                portaSerial.Dispose();
                portaSerial = null;
                button_CriarCOM.Text = "Conecta";
                textBox_StatusCOM.Text = $"Porta fechada.";
                comboBox_PortaCOM.Enabled = true;
                comboBox_BaudRate.Enabled = true;
            }
            
        }

        private void comboBox_PortaCOM_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_PortaCOM.SelectedItem?.ToString() == "Atualizar")
            {
                comboBox_PortaCOM.Items.Clear();
                comboBox_PortaCOM.Items.AddRange(SerialPort.GetPortNames());
                comboBox_PortaCOM.Items.Add("Atualizar");
            }
        }

        /*private void PortaSerial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesToRead = portaSerial.BytesToRead;
            byte[] buffer = new byte[bytesToRead];
            portaSerial.Read(buffer, 0, bytesToRead);

            bufferRecebido.AddRange(buffer);

            // Tenta processar o pacote se tiver pelo menos 8 bytes (mínimo de um pacote válido Modbus RTU)
            while (bufferRecebido.Count >= 8)
            {
                // Verifica se o endereço corresponde ao escravo
                if (bufferRecebido[0] != EnderecoEscravo)
                {
                    bufferRecebido.RemoveAt(0); // descarta byte inválido
                    continue;
                }

                int tamanhoEsperado = 8; // mínimo
                byte funcao = bufferRecebido[1];

                if (funcao == 0x03 || funcao == 0x06)
                    tamanhoEsperado = 8; // ambas funções têm 8 bytes

                if (bufferRecebido.Count < tamanhoEsperado)
                    return; // aguarda mais bytes

                byte[] pacote = bufferRecebido.Take(tamanhoEsperado).ToArray();
                bufferRecebido.RemoveRange(0, tamanhoEsperado);

                ushort crcCalculado = CalcularCRC(pacote, tamanhoEsperado - 2);
                ushort crcRecebido = (ushort)(pacote[tamanhoEsperado - 2] | (pacote[tamanhoEsperado - 1] << 8));

                if (crcCalculado != crcRecebido)
                {
                    Invoke(new Action(() => textBox_StatusCOM.AppendText("\r\nCRC inválido.")));
                    continue;
                }

                byte[] resposta = ProcessarModbusRTU(pacote);
                if (resposta != null)
                    portaSerial.Write(resposta, 0, resposta.Length);
            }
        }*/

        private void PortaSerial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesToRead = portaSerial.BytesToRead;
            byte[] buffer = new byte[bytesToRead];
            portaSerial.Read(buffer, 0, bytesToRead);

            if (buffer.Length >= 1)
            {
                byte endereco = buffer[0];
                foreach (var slave in dispositivos.Values)
                {
                    if (slave.Endereco == endereco)
                    {
                        byte[] resposta = slave.ProcessarBuffer(buffer);
                        if (resposta != null)
                        {
                            portaSerial.Write(resposta, 0, resposta.Length);
                            Invoke(new Action(() => textBox_StatusCOM.AppendText("\r\nResposta enviada.")));
                        }
                        break;
                    }
                }
            }
        }

        private void SalvarTodosDispositivos()
        {
            string pasta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dados");

            foreach (var item in dispositivos)
            {
                string nome = item.Key;
                var slave = item.Value;

                var dados = new DispositivoData
                {
                    ID_Slave = slave.Endereco,
                    Registros = slave.Registros,
                    Entradas = slave.Entradas,
                    Saidas = slave.Saidas
                };

                string caminho = Path.Combine(pasta, nome + ".json");
                string json = JsonSerializer.Serialize(dados, new JsonSerializerOptions { WriteIndented = true });

                try
                {
                    File.WriteAllText(caminho, json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao salvar {nome}.json: {ex.Message}");
                }
            }
        }

        private void Principal_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (portaSerial != null)
            {
                var resultado = MessageBox.Show("Deseja salvar os dados dos dispositivos antes de sair?", "Salvar Dados", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (resultado == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                if (resultado == DialogResult.Yes)
                {
                    SalvarTodosDispositivos();
                }
            }
        }



    }
}
