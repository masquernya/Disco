import s from './tutorial.module.css';
import UserListCard from "../userListCard";
import {useEffect, useState} from "react";
import TutorialPage from "./tutorialPage";
import Link from "next/link";
import api from "../../lib/api";
const items = [
  {
    title: 'How does this work?',
    description: <div>
      <p>DiscoFriends is a platform for discovering friends to add on Discord.</p>
      <p>DiscoFriends uses "Tags" to match you with other people.</p>
      <p>You can add up to 100 tags that are used to match you with other users.</p>
      <p>These tags should be things you like, such as hobbies or a video game, but they can also be descriptors of yourself such as your country or job.</p>
    </div>,
  },
  {
    description: <div>
      <p>Unlike similar platforms, users aren't matched in real-time. Anybody you see on the list may not be online when you add them, so be patient.</p>
    </div>
  },
  {
    description: <div>
      <div className='row'>
        <div className='col-12 col-lg-8 mx-auto'>
            <UserListCard user={{accountId: 1, username: 'ExampleUser', displayName: 'ExampleUser', description: 'This is an example description.', tags: [{displayTag: 'Tag1'}, {displayTag: 'Tag2'}], socialMedia: [{type: 'Discord'}], avatar: {imageUrl: '/dog-256.jpg'}}} />
        </div>
      </div>
      <div className='row mt-4'>
        <div className='col-12 col-lg-12 mx-auto'>
          <p>
            For each user, you can either "Accept" or "Decline" them.
          </p>
          <p>
            If you hit "Accept" on their profile, and the other person hits "Accept" on your profile, your discords will be shared with eachother.
          </p>
          <p>
            "Accepting" and "Declining" users is completely private - nobody will know you accepted or declined them unless you both "Accept" each other.
          </p>
          <p>
            You can also click the "Report" button below user accounts to report them if they break our <Link href={'/terms-of-service'}>Terms of Service</Link>.
          </p>
        </div>
      </div>
    </div>
  },
  {
    description: <div>
      <p>We try to keep our service as safe as possible without violating user privacy, but there are some things you should keep in mind for online safety:</p>
      <ul>
        <li className='mb-4'>We don't verify anybody who joins our site. You can put any age, gender, pronouns, or any information you want, and you can change it at any time.</li>
        <li className='mb-4'>You should not send your address, city, passwords, payment info, or money to anyone online.</li>
        <li className='mb-4'>"Sextortion" scams are very common on platforms like Discord. To stay safe, <span className='fw-bold'>do not send anyone intimate pictures of yourself (or others).</span> For more information, the FBI has some great resources, such as <a href='https://www.fbi.gov/video-repository/newss-what-is-sextortion/view' target='_blank' rel='nofollow noopener noreferrer'>this video</a> or <a target='_blank' rel='nofollow noopener noreferrer' href='https://www.fbi.gov/how-we-can-help-you/safety-resources/scams-and-safety/common-scams-and-crimes/sextortion'>post</a> explaining "Sextortion".</li>
        <li className='mb-4'>DiscoFriends isn't meant for online dating. It doesn't have the same safeguards or required features that real online dating platforms have.</li>
      </ul>
    </div>,
    delay: 15*1000,
  },
  {
    description: <div>
      <p>In order to get started, hit the button below to link your Discord account to DiscoFriends. This is required to start matching with people.</p>
      <p>After linking your account, you can add your tags and information in <Link href={'/me'}>settings</Link>.</p>
    </div>,
  },
]

export default function OnboardTutorial() {
  const [position, setPosition] = useState(0);
  const [discordUrl, setDiscordUrl] = useState(null);
  const [locked, setLocked] = useState(false);
  useEffect(() => {
    api.request('/api/user/DiscordLinkUrl', {
      method: 'POST',
    }).then(data => {
      setDiscordUrl(data.body.redirectUrl);
    })
  }, []);

  const value = items[position];
  const label = !items[position+1] ? 'Get Started' : 'Next';
  return <div className={s.tutorial}>
    <TutorialPage description={value.description} />
    <button disabled={locked} className={s.nextButton + ' text-uppercase'} onClick={() => {
      if (!items[position+1]) {
        window.location.href = discordUrl;
        return;
      }
      setPosition(position+1);
      if (items[position+1].delay) {
        setLocked(true);
        setTimeout(() => {
          setLocked(false);
        }, items[position+1].delay)
      }
    }}>{label}</button>
  </div>
}