import {getUser} from "../../lib/globalState";
import Description from "./description";
import DisplayName from "./displayName";
import Age from "./age";
import Gender from "./gender";
import Pronouns from "./pronouns";
import Tags from "./tags";
import UserListCard from "../userListCard";
import {useEffect, useState} from "react";
import api from "../../lib/api";
import Discord from "./discord";
import Avatar from "./avatar";
import Password from "./password";
import DeleteAccount from "./deleteAccount";
import Matrix from "./matrix";
import s from './me.module.css';
import {useRouter} from "next/router";
import modal from '../../styles/Modal.module.css';

export default function Me(props) {
  const router = useRouter();
  const [firstLinkVisible, setFirstLinkVisible] = useState(false);
  const [avatar, setAvatar] = useState(null);
  const [discord, setDiscord] = useState(null);
  const [matrix, setMatrix] = useState(null);
  const [showPreview, setShowPreview] = useState(false); // for mobile
  useEffect(() => {
    api.request('/api/user/Avatar').then(data => {
      setAvatar(data.body);
    });
    api.request('/api/user/Discord').then(data => {
      setDiscord(data.body);
    })
    api.request('/api/user/Matrix').then(data => {
      setMatrix(data.body);
    })
  }, []);

  useEffect(() => {
    if (router.query.link) {
      setFirstLinkVisible(true);
    }
  }, [router.query]);

  const socialMedia = [];
  if (discord) {
    socialMedia.push({
      type: 'Discord',
      displayString: discord.displayString,
    });
  }

  if (matrix) {
    socialMedia.push({
      type: 'Matrix',
      displayString: '@' + matrix.name + ':' + matrix.domain,
    })
  }

  if (!getUser() || !getUser().data)
    return null;

  const user = getUser().data;

  return <div className='container min-vh-100'>
    {
      (firstLinkVisible && (!discord && !matrix)) ? <div className={modal.modal}>
        <div className={modal.content}>
          <h3 className='fw-bold'>Account Link</h3>
          <p>In order to start getting matches, you'll have to link an account from a supported social media site. This is shown to other users if you both "Like" each other.</p>
          <Discord discord={discord}  setDiscord={setDiscord} matrix={matrix} setAvatar={setAvatar} />
          <Matrix matrix={matrix} setMatrix={setMatrix} discord={discord} setAvatar={setAvatar} />
          <button className='btn btn-outline-secondary mt-0' onClick={() => {
            setFirstLinkVisible(false);
          }}>Dismiss</button>
        </div>
      </div> : null
    }
    <div className='row mt-4'>
      <div className='col-12 d-block d-lg-none'>
        <button className={s.saveButton + ' mb-4'} onClick={() => {
          setShowPreview(!showPreview);
        }}>{showPreview ? 'Hide Preview' : 'Show Preview'}</button>
      </div>
      <div className={'col-12 col-lg-6 mx-auto ' + (showPreview ? 'd-none' : 'd-block')}>
        <h3 className='fw-bold'>{'@'+getUser().data.username}</h3>
        <DisplayName />
        <Description />
        <Age />
        <Gender />
        <Pronouns />
        <Tags />
        <Discord discord={discord}  setDiscord={setDiscord} matrix={matrix} setAvatar={setAvatar} />
        <Matrix matrix={matrix} setMatrix={setMatrix} discord={discord} setAvatar={setAvatar} />
        {(discord || matrix) ? <Avatar avatar={avatar} setAvatar={setAvatar} discord={discord} matrix={matrix} /> : null}
        <Password />
        <DeleteAccount />
      </div>
      <div className={'col-12 col-lg-6 ' + (showPreview ? '' : 'd-none d-lg-block')}>
        <h3 className='fw-bold'>Preview</h3>
        <div>
          <UserListCard user={{
            username: user.username,
            displayName: user.displayName,
            description: user.description,
            pronouns: user.pronouns,
            age: user.age,
            gender: user.gender,
            socialMedia: socialMedia,
            tags: user.tags.map(v => {
              return {
                displayTag: v.tag,
              }
            }),
            avatar: avatar ? {
              imageUrl: avatar.url,
            } : null,
          }} />
        </div>
      </div>
    </div>
  </div>
}