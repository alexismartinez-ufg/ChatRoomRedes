using Microsoft.AspNetCore.SignalR;
using System.ComponentModel;
using System;
using Microsoft.EntityFrameworkCore;
using ChatRoom.Models;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace ChatRoom
{
    public class ChatHub:Hub
    {
        private readonly MyDbContext context;

        public ChatHub(MyDbContext context) 
        {
            this.context = context;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public async Task SendAllMessages()
        {
            var messages = await context.Mensaje.ToListAsync();
            var decryptedMessages = new List<Mensaje>();

            foreach (var message in messages)
            {
                var decryptedContent = await Decrypt(message.Contenido);
                var decryptedMessage = new Mensaje
                {
                    IdMensaje = message.IdMensaje,
                    Contenido = decryptedContent,
                    FechaHora = message.FechaHora,
                    NombreRemitente = message.NombreRemitente
                };

                decryptedMessages.Add(decryptedMessage);
            }

            var messagesJson = JsonConvert.SerializeObject(decryptedMessages);
            await Clients.Caller.SendAsync("LoadMessages", messagesJson);

            await SendLogToClient($"Se obtuvieron {decryptedMessages.Count} mensajes");
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var usuario = await context.Usuario.FirstOrDefaultAsync(x => x.ConnectionId == Context.ConnectionId);
            if (usuario != null)
            {
                context.Usuario.Remove(usuario);

                await context.SaveChangesAsync();
            }
        }

        public async Task SendMessage(string room, string user, string message)
        {
            await Clients.Group(room).SendAsync("ReceiveMessage", user, message);
        }

        public async Task AddToGroup(string room)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("ShowHo", $"Alguien se conecto {Context.ConnectionId}");
        }

        public async Task ContainsUser()
        {
            var tieneNombre = false;
            var nombre = "";

            if (context.Usuario.Any()) tieneNombre = context.Usuario.Any(x => x.ConnectionId == Context.ConnectionId && !string.IsNullOrEmpty(x.Name));

            if (tieneNombre) nombre = context.Usuario.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId)?.Name;

            await Clients.Client(Context.ConnectionId).SendAsync("IsConnected", tieneNombre, nombre);
        }

        public async Task SendMessageToClient(string Usuario, string mensaje)
        {

            if (context.Usuario.Any(x => x.ConnectionId == Context.ConnectionId))
            {
                var usuario = await context.Usuario.FirstOrDefaultAsync(x => x.ConnectionId == Context.ConnectionId);

                if(usuario != null && usuario.Name == Usuario)
                {
                    var newMensaje = new Mensaje()
                    {
                        Contenido = await Encrypt(mensaje),
                        FechaHora = DateTime.UtcNow,
                        NombreRemitente = usuario.Name
                    };

                    await context.Mensaje.AddAsync(newMensaje);
                    await context.SaveChangesAsync();

                    await Clients.All.SendAsync("SendMessageToClients", await Decrypt(newMensaje.Contenido), newMensaje.FechaHora, newMensaje.NombreRemitente);
                }
                else
                {
                    //enviarmensaje de error
                }
            }
            else
            {
                //enviar mensaje de error
            }
        }

        public async Task SetName(string name)
        {
            var usuario = new User();

            if (context.Usuario.Any(x => x.ConnectionId == Context.ConnectionId))
            {
                usuario = await context.Usuario.FirstOrDefaultAsync(x => x.ConnectionId == Context.ConnectionId);

                if (usuario != null)
                {

                    usuario.Name = name;
                    await Clients.Client(Context.ConnectionId).SendAsync("IsConnected", true, name);

                }
            }
            else
            {

                usuario.ConnectionId = Context.ConnectionId; usuario.Name = name;
                await context.Usuario.AddAsync(usuario);
                await Clients.Client(Context.ConnectionId).SendAsync("IsConnected", true, name);
                
                
            }

            await context.SaveChangesAsync();
        }

        public async Task SendLogToClient(string mensaje)
        {
            await Clients.Caller.SendAsync("ReceiveLog", mensaje, DateTime.UtcNow);
        }

        public async Task SendLogToClients(string mensaje)
        {
            await Clients.All.SendAsync("ReceiveLog", mensaje, DateTime.UtcNow);
        }

        public async Task<string> Encrypt(string mensaje)
        {

            string hash = "PruebaMD5";
            byte[] data = UTF8Encoding.UTF8.GetBytes(mensaje);

            await SendLogToClient($"Encriptando key con MD5 {hash}");

            MD5 md5 = MD5.Create();
            TripleDES triples = TripleDES.Create();

            triples.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));
            triples.Mode = CipherMode.ECB;

            await SendLogToClient($"Key encriptada con MD5 = {BitConverter.ToString(triples.Key).Replace("-", "").ToLower()}");

            ICryptoTransform transform = triples.CreateEncryptor();
            byte[] result = transform.TransformFinalBlock(data, 0, data.Length);

            var encripted = Convert.ToBase64String(result);

            await SendLogToClient($"Mensaje encriptado: {encripted}");

            return encripted;
        }

        public async Task<string> Decrypt(string mensajeEn)
        {

            string hash = "PruebaMD5";
            byte[] data = Convert.FromBase64String(mensajeEn);

            await SendLogToClient($"Desencriptando con Key = {hash}");

            MD5 md5 = MD5.Create();
            TripleDES triples = TripleDES.Create();

            triples.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));
            triples.Mode = CipherMode.ECB;

            await SendLogToClient($"Desencriptando con Key resultante en MD5 = {BitConverter.ToString(triples.Key).Replace("-", "").ToLower()}");

            ICryptoTransform transform = triples.CreateDecryptor();
            byte[] result = transform.TransformFinalBlock(data, 0, data.Length);

            var decriptstring = UTF8Encoding.UTF8.GetString(result);

            await SendLogToClient($"Mensaje Desencriptado: {decriptstring}");

            return decriptstring;
        }
    }
}
