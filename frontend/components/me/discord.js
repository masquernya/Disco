import s from "./me.module.css";
import api from "../../lib/api";
import {useState} from "react";

export default function Discord({discord, setDiscord, matrix, setAvatar}) {
  const [feedback, setFeedback] = useState(null);

  return <div className={s.meSection}>
    <h4 className={s.subtitle + ' text-uppercase'}>Discord</h4>
    {
      feedback ? <p className='text-danger'>{feedback}</p> : null
    }
    {
      discord ? <div>
        <p className=''>Your account is currently linked to {discord.displayString} (ID: {discord.discordId})</p>
        <button className='btn btn-outline-danger btn-sm' onClick={() => {
          api.request('/api/user/Discord', {
            method: 'DELETE',
          }).then(() => {
            setDiscord(null);
            setAvatar(null);
          }).catch(e => {
            setFeedback(e.message);
          })
        }}>Unlink Account</button>
        {!matrix ? <p className='fst-italic mt-4'>Unlinking your account will restrict access to DiscoFriends until you link a Discord or Matrix account.</p> : null}
          </div> : <div>
        <button className={s.saveButton} onClick={() => {
          api.request('/api/user/DiscordLinkUrl', {
            method: 'POST',
          }).then(data => {
            window.location.href = data.body.redirectUrl;
          })
        }}>Link Account</button>
      </div>
    }
  </div>
}