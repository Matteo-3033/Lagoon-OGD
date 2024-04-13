using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using UnityEngine;
using Utils;

namespace Network.Master
{
    public class MatchRequestPacket : SerializablePacket
    {
        public int RoundsCnt { get; set; }
        
        public override void ToBinaryWriter(EndianBinaryWriter writer)
        {
            writer.Write(RoundsCnt);
        }

        public override void FromBinaryReader(EndianBinaryReader reader)
        {
            RoundsCnt = reader.ReadInt32();
        }
    }
    
    public class RoomsModule : MasterServerToolkit.MasterServer.RoomsModule
    {
        public const string MatchStarted = "-room.match_started";
        public const string RoundsCntKey = "-room.rounds_cnt";
        public const string RoundsKey = "-room.rounds";
        private const string RoomMasterUserKey = "-room.masterUser";
        
        [SerializeField] private RoundConfiguration[] roundPool;
        
        private readonly ConcurrentDictionary<string, IIncomingMessage> waitingPlayers = new();
        
        private SpawnersModule spawnersModule;

        public override void Initialize(IServer server)
        {
            base.Initialize(server);
            logger.Debug("Rooms module initialized");
            
            spawnersModule = server.GetModule<SpawnersModule>();

            if (!spawnersModule)
                logger.Error($"{GetType().Name} was set to use {nameof(SpawnersModule)}, but {nameof(SpawnersModule)} was not found." +
                             $"In this case, you will not be able to get regions list");
            
            server.RegisterMessageHandler(Messages.OpCodes.GetMatch, GetMatchRequestHandler);

            OnRoomRegisteredEvent += OnRoomRegistered;
        }

        private void GetMatchRequestHandler(IIncomingMessage message)
        {
            logger.Debug("Received match request");
            
            var data = message.AsPacket<MatchRequestPacket>();
            if (data.RoundsCnt % 2 == 0)
                data.RoundsCnt++;

            var player = message.Peer.GetExtension<IUserPeerExtension>();
            if (player.JoinedRoomID > 0)
            {
                message.Respond("You are already in a room", ResponseStatus.NotHandled);
                return;
            }
            
            if (waitingPlayers.ContainsKey(player.Username))
            {
                message.Respond("You are already in queue", ResponseStatus.NotHandled);
                return;
            }

            var room = roomsList.Values
                    .FirstOrDefault(r => r.Options.IsPublic && !r.Options.CustomOptions.AsBool(MatchStarted, false) &&
                                         r.Options.CustomOptions.AsInt(RoundsCntKey) == data.RoundsCnt);   

            if (room != null) {
                logger.Debug("Joining existing room");
                
                room.GetAccess(message.Peer, new MstProperties(), (packet, error) =>
                {
                    if (packet == null)
                    {
                        message.Respond(error, ResponseStatus.Unauthorized);
                        return;
                    }

                    message.Respond(packet, ResponseStatus.Success);
                });
            }
            else
            {
                waitingPlayers[player.Username] = message;
                
                var options = new MstProperties();
                options.Add(RoundsCntKey, data.RoundsCnt);
                
                var rounds = roundPool.OrderBy(_ => Guid.NewGuid()).Take(data.RoundsCnt).Select(r => r.name).ToArray();
                options.Add(RoundsKey, string.Join(' ', rounds));
                
                options.Add(RoomMasterUserKey, player.Username);
                
                logger.Debug("Creating room with rounds" + string.Join(", ", rounds));
                spawnersModule.Spawn(options);
            }
        }
        
        private void OnRoomRegistered(RegisteredRoom room)
        {
            logger.Debug("Room registered");
            
            var username = room.Options.CustomOptions.AsString(RoomMasterUserKey, "");
            if (string.IsNullOrEmpty(username)) return;
            
            logger.Debug("Room master: " + username);
            
            StartCoroutine(DoOnRoomRegistered(room, username));
        }

        private IEnumerator DoOnRoomRegistered(RegisteredRoom room, string username)
        {
            yield return new WaitForSeconds(1);
            
            if (!waitingPlayers.TryRemove(username, out var message)) yield return null;
            
            room.GetAccess(message.Peer, new MstProperties(), (packet, error) =>
            {
                if (packet == null)
                {
                    message.Respond(error, ResponseStatus.Unauthorized);
                    return;
                }

                message.Respond(packet, ResponseStatus.Success);
            });
        }

        protected override void GetRoomAccessRequestHandler(IIncomingMessage message)
        {
        }
    }
    
    public static class GuidExtensions
    {
        public static Guid ToGuid(this string id)
        {
            var provider = new MD5CryptoServiceProvider();
            var inputBytes = Encoding.Default.GetBytes(id);
            var hashBytes = provider.ComputeHash(inputBytes);

            return new Guid(hashBytes);
        }
    }
    
    public static class RoundExtensions
    {
        public static RoundConfiguration[] GetRounds(this string rounds)
        {
            var roundNames = rounds.Split(' ');
            return roundNames.Select(name => Resources.Load<RoundConfiguration>($"Rounds/{name}")).ToArray();
        }
    }
}