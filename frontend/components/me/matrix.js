import s from "./me.module.css";
import api from "../../lib/api";
import {useState} from "react";
import modal from '../../styles/Modal.module.css';

export default function Matrix({matrix, setMatrix, discord, setAvatar}) {
  const [feedback, setFeedback] = useState(null);
  const [matrixToAdd, setMatrixToAdd] = useState('');
  const [showAvatarWarning, setShowAvatarWarning] = useState(false);

  return <div className={s.meSection}>
    {
      showAvatarWarning ? <div className={modal.modal}>
        <div className={modal.content}>
          <h3 className='fw-bold'>Avatar Approval</h3>
          <p>It may take a few minutes for your profile picture to be approved. Until then, or if your picture is rejected, your profile picture will show up as a question mark.</p>
          <button className={s.saveButton} onClick={() => {
            setShowAvatarWarning(false);
          }}>Ok</button>
        </div>
      </div> : null
    }
    <h4 className={s.subtitle + ' text-uppercase'}>Matrix</h4>
    {
      feedback ? <p className='text-danger'>{feedback}</p> : null
    }
    {
      matrix ? <div>
        <p className=''>Your account is currently linked to {'@' + matrix.name + ':' + matrix.domain}.</p>
        <button className='btn btn-outline-danger btn-sm' onClick={() => {
          api.request('/api/user/Matrix', {
            method: 'DELETE',
          }).then(() => {
            setMatrix(null);
          }).catch(e => {
            setFeedback(e.message);
          })
        }}>Unlink Account</button>
          {!discord ? <p className='fst-italic mt-4'>Unlinking your account will restrict access to DiscoFriends until you link a Matrix or Discord account.</p> : null}
      </div> : <div>
        <input disabled={false} className={s.description} placeholder='Matrix username (e.g. @example:matrix.org)' onChange={e => {
          setMatrixToAdd(e.currentTarget.value);
        }}/>
        <button className={s.saveButton} onClick={() => {
          setFeedback(null);
          api.request('/api/user/Matrix', {
            method: 'POST',
            body: {
              username: matrixToAdd,
            }
          }).then(data => {
            // Fetch new matrix, then set
            return api.request('/api/user/Matrix').then(data => {
              setMatrix(data.body);
              // Get new avatar. It's possible it was updated.
              api.request('/api/user/Avatar').then(data => {
                if (!data.body)
                  setShowAvatarWarning(true); // Avatar was not auto approved
                setAvatar(data.body);
              });
            })
          }).catch(err => {
            if (err.code === 'InvalidMatrixUsernameException') {
              setFeedback('Username is invalid. Make sure you include the domain, such as "matrix.org".');
              return;
            }
            if (err.code === 'MatrixUserNotFoundException') {
              setFeedback('User does not exist.');
              return;
            }
            setFeedback(err.message);
          })
        }}>Link Account</button>
      </div>
    }
  </div>
}