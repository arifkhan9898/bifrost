﻿using System;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Linq;
using PagarMe;
using PagarMe.Mpos;

namespace PaymentTest
{
    public class PaymentProcessor
    {
        private readonly SerialPort _port;
        private readonly Mpos _mpos;

        public PaymentProcessor(string device)
        {
            _port = new SerialPort(device, 19200, Parity.None, 8, StopBits.One);
            _port.ErrorReceived += (object sender, SerialErrorReceivedEventArgs e) => Console.WriteLine(e.EventType.ToString());
            _port.Open();

            _mpos = new Mpos(_port.BaseStream, "ek_live_IiZGjjXdxDug8t8xRtEFas0dke6I7H");
            _mpos.NotificationReceived += (sender, e) => Console.WriteLine("Status: {0}", e);

            PagarMeService.DefaultApiEndpoint = "http://localhost:3000";
            PagarMeService.DefaultEncryptionKey = "ek_live_IiZGjjXdxDug8t8xRtEFas0dke6I7H";
            PagarMeService.DefaultApiKey = "ak_live_SIfpRudJkS04ga5pQxag8Sz8Fvdr4z";
        }

        public async Task Initialize()
        {
            await _mpos.Initialize();

            await _mpos.SynchronizeTables();
        }

        public async Task<Transaction> Pay(int amount)
        {
            var result = await _mpos.ProcessPayment(amount);

            var transaction = new Transaction
                {
                    CardHash = result.CardHash,
                    Amount = amount,
                    ShouldCapture = false
                };

            await transaction.SaveAsync();

            await _mpos.FinishTransaction(Int32.Parse(transaction.AcquirerResponseCode), transaction["card_emv_response"].ToString());

            return transaction;
        }
    }
}

