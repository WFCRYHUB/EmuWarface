using EmuWarface.Core;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

namespace EmuWarface.Game.Notifications
{
    public partial class Notification
    {
        /*
        <sync_notifications>
          <notif id="6016331252" type="64" confirmation="1" from_jid="masterserver@warface/pve_054_r1" message="">
            <invitation target="Покажите">
              <initiator_info online_id="747193555@warface/GameClient" profile_id="25785570" is_online="1" name="МрЛоки1965Герда" clan_name="" experience="0" badge="4294967295" mark="4294967295" stripe="4294967295" />
            </invitation>
          </notif>
        </sync_notifications>
         */

        /*
         
<notif id='0' type='131072' confirmation='0' from_jid='masterserver@warface/pve_059_r3' message=''>
<new_rank_reached old_rank='1' new_rank='2'/>
</notif>
<notif id='3177524456' type='2048' confirmation='1' from_jid='masterserver@warface/pve_059_r3' message=''>
<give_money currency='game_money' type='0' amount='100' notify='1'/>
</notif>
<notif id='0' type='4' confirmation='0' from_jid='masterserver@warface/pve_059_r3' message=''>
<achievement achievement_id='54' progress='2' completion_time='0'/>
</notif>
<notif id='0' type='4' confirmation='0' from_jid='masterserver@warface/pve_059_r3' message=''>
<achievement achievement_id='55' progress='2' completion_time='0'/>
</notif>
<notif id='0' type='4' confirmation='0' from_jid='masterserver@warface/pve_059_r3' message=''>
<achievement achievement_id='58' progress='2' completion_time='0'/>
</notif>
<notif id='0' type='4' confirmation='0' from_jid='masterserver@warface/pve_059_r3' message=''>
<achievement achievement_id='413' progress='2' completion_time='0'/>
</notif>
<notif id='0' type='4' confirmation='0' from_jid='masterserver@warface/pve_059_r3' message=''>
<achievement achievement_id='499' progress='2' completion_time='0'/>
</notif>
<notif id='3177524459' type='256' confirmation='1' from_jid='masterserver@warface/pve_059_r3' message=''>
<give_item name='smg12_shop' offer_type='Permanent' notify='1'>
<sponsor_item/>
</give_item>
</notif>
<notif id='3177524460' type='1048576' confirmation='1' from_jid='masterserver@warface/pve_059_r3' message=''>
<mission_unlocked_message silent='0' unlocked_mission='easymission'/>
</notif>
<notif id='3177528405' type='262144' confirmation='1' from_jid='masterserver@warface/pve_059_r3' message=''>
<message data='@Medic_unlocked'/>
</notif>
<notif id='3177528403' type='2048' confirmation='1' from_jid='masterserver@warface/pve_059_r3' message=''>
<give_money currency='game_money' type='0' amount='3000' notify='1'/>
</notif>
<notif id='3177528404' type='256' confirmation='1' from_jid='masterserver@warface/pve_059_r3' message=''>
<give_item name='shg13_shop' offer_type='Expiration' notify='1' extended_time='720'/>
</notif>
<notif id='3177528406' type='256' confirmation='1' from_jid='masterserver@warface/pve_059_r3' message=''>
<give_item name='pt02_shop' offer_type='Expiration' notify='1' extended_time='336'/>
</notif>
<notif id='0' type='128' confirmation='0' from_jid='masterserver@warface/pve_018_r1' message=''>
<invite_result profile_id='25785570' jid='747193555@warface/GameClient' nickname='МрЛоки1965Герда' status='17' location='' experience='0' result='0' invite_date='0'/>
</notif>*/
        public ulong Id                 { get; set; }
        //public long SecondsLeftToExpire { get; set; }
        public long ExpirationTimeUtc { get; set; }
        public NotificationType Type    { get; set; }
        public XmlElement Element       { get; set; }

        public long SecondsLeftToExpire
        {
            get
            {
                return Math.Clamp(ExpirationTimeUtc - DateTimeOffset.UtcNow.ToUnixTimeSeconds(), 0, long.MaxValue);
            }
            set
            {
                ExpirationTimeUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + value;
            }
        }

        public bool Confirmation => Id != 0;

        public Notification(NotificationType type, long expirationTimeUtc = 0)
        {
            Type                = type;
            ExpirationTimeUtc   = expirationTimeUtc;
        }

        public Notification()
        {

        }

        public XmlElement Serialize()
        {
            XmlElement notif = Xml.Element("notif")
                .Attr("id", Id)
                .Attr("type",                   (int)Type)
                .Attr("confirmation",           Convert.ToByte(Confirmation))
                .Attr("from_jid",               "masterserver@warface/emuwarface") //TODO test
                .Attr("seconds_left_to_expire", SecondsLeftToExpire)
                .Attr("message", "");

            notif.Child(Element);

            return notif;
        }

        public static Notification ParseDataRow(DataRow row)
        {
            return new Notification
            {
                Id                      = (ulong)row["id"],
                ExpirationTimeUtc       = (long)row["expiration_time_utc"],
                Type                    = (NotificationType)row["type"],
                Element                 = Xml.Parse((string)row["data"]),
            };
        }

        public static List<Notification> GetNotifications(ulong profile_id)
        {
            List<Notification> notifs = new List<Notification>();

#if DEBUG
            return notifs;
#endif

            var result = SQL.QueryRead($"SELECT * FROM emu_notifications WHERE profile_id={profile_id}");

            foreach (DataRow row in result.Rows)
            {
                var notif = Notification.ParseDataRow(row);

                if(notif.SecondsLeftToExpire == 0)
                {
                    RemoveNotification(profile_id, notif.Id);
                }
                else
                {
                    notifs.Add(notif);
                }
            }

            return notifs;
        }

        public static Notification GetNotification(ulong profile_id, ulong id)
        {
            DataTable db = SQL.QueryRead($"SELECT * FROM emu_notifications WHERE id={id} AND profile_id={profile_id}");

            if (db.Rows.Count != 1)
                return null;

            return Notification.ParseDataRow(db.Rows[0]);
        }

        public static void RemoveNotification(ulong profile_id, ulong id)
        {
            SQL.Query($"DELETE FROM emu_notifications WHERE profile_id={profile_id} AND id={id}");
        }

        public static void AddNotification(ulong profile_id, Notification notif)
        {
            MySqlCommand cmd = new MySqlCommand("INSERT INTO emu_notifications (`profile_id`, `type`, `confirmation`, `data`, `expiration_time_utc`) VALUES " +
                $"(@profile_id, @type, @confirmation, @data, @expiration_time_utc); SELECT LAST_INSERT_ID();");

            cmd.Parameters.AddWithValue("@profile_id", profile_id);
            cmd.Parameters.AddWithValue("@type", (int)notif.Type);
            cmd.Parameters.AddWithValue("@confirmation", 1);
            cmd.Parameters.AddWithValue("@data", notif.Element.OuterXml);
            cmd.Parameters.AddWithValue("@expiration_time_utc", notif.ExpirationTimeUtc);

            DataTable dt = SQL.QueryRead(cmd);

            notif.Id = Convert.ToUInt64(dt.Rows[0][0]);
        }

        public static void SyncNotifications(ulong profile_id, params Notification[] notifs)
        {
            Client client = null;
            lock (Server.Clients)
            {
                client = Server.Clients.FirstOrDefault(x => x.ProfileId == profile_id);
            }

            if (client != null && client.Profile != null)
                SyncNotifications(client, notifs);
        }

        public static void SyncNotifications(Client client, params Notification[] notifs)
        {
            if (client == null || client.Profile == null)
                return;

            XmlElement response = Xml.Element("sync_notifications");

            if (notifs.Length == 0)
            {
                GetNotifications(client.ProfileId).ForEach(x => response.Child(x.Serialize()));
            }
            else
            {
                foreach (var notif in notifs)
                {
                    response.Child(notif.Serialize());
                }
            }

            //Iq iq = new Iq(IqType.Get, client.Jid, Server.Channels.First().Jid);
            //client.QueryGet(iq.SetQuery(response));
            client.QueryGet(response);
        }
    }
}
