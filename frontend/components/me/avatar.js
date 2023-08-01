import s from "./me.module.css";
import api from "../../lib/api";
import {useState} from "react";
import {getUser} from "../../lib/globalState";

export default function Avatar({avatar, setAvatar}) {
  const [feedback, setFeedback] = useState(null);

  return <div className={s.meSection}>
    <h4 className={s.subtitle + ' text-uppercase'}>Avatar</h4>
    <p className={s.subtitleHelp}>Your avatar is fetched from discord when you link your account. You cannot set a custom avatar. If you changed your avatar after linking your account, you can click the button below to update your avatar.</p>
    {
      feedback ? <p className='text-danger'>{feedback}</p> : null
    }
    <div>
      <button className={s.saveButton} onClick={() => {
        api.request('/api/user/DiscordLinkUrl', {
          method: 'POST',
        }).then(data => {
          window.location.href = data.body.redirectUrl;
        })
      }}>Update Avatar</button>
    </div>
    <div>
      {avatar ?
        <button className='btn btn-sm btn-outline-danger mt-4' onClick={() => {
          api.request('/api/user/Avatar', {
            method: 'DELETE',
          }).then(() => {
            setAvatar(null);
          })
        }}>Delete Avatar</button> : null
      }
    </div>
  </div>
}