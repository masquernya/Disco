import s from "./me.module.css";
import api from "../../lib/api";
import {useState} from "react";
import {getUser} from "../../lib/globalState";

export default function Avatar({avatar, setAvatar, discord, matrix}) {
  const [feedback, setFeedback] = useState(null);
  const [settingAvatar, setSettingAvatar] = useState(false);

  return <div className={s.meSection}>
    <h4 className={s.subtitle + ' text-uppercase'}>Avatar</h4>
    <p className={s.subtitleHelp}>Your avatar is fetched from Discord or Matrix when you link your account. You cannot set a custom avatar. If you changed your avatar after linking your account, you can click the button below to update your avatar.</p>
    {
      feedback ? <p className='text-danger'>{feedback}</p> : null
    }
    <div>
      {
        discord ? <button disabled={settingAvatar} className={s.saveButton} onClick={() => {
          setSettingAvatar(true);
          api.request('/api/user/DiscordLinkUrl', {
            method: 'POST',
          }).then(data => {
            window.location.href = data.body.redirectUrl;
          })
        }}>Update Avatar from Discord</button> : null
      }
      {
        matrix ? <button disabled={settingAvatar} className={s.saveButton} onClick={() => {
          setSettingAvatar(true);
          api.request('/api/user/Matrix', {
            method: 'POST',
            body: {
              username: '@' + matrix.name + ':' + matrix.domain,
            }
          }).then(data => {
            return api.request('/api/user/Avatar').then(data => {
              setAvatar(data.body);
            });
          }).catch(err => setFeedback(err.message))
            .finally(() => {
              setSettingAvatar(false);
            })

        }}>Update Avatar from Matrix</button> : null
      }
    </div>
    <div>
      {avatar ?
        <button disabled={settingAvatar} className='btn btn-sm btn-outline-danger mt-4' onClick={() => {
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