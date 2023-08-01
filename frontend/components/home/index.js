import UserListCard from "../userListCard";
import s from './home.module.css';
import Link from "next/link";
import reportEvent from "../../events/eventBase";

export default function Home() {
  return <div className='min-vh-100'>
    <div className='container'>
      <div className='row mt-4'>
        <div className='col-12 col-lg-6'>
          <h1 className={s.header}>Make Discord Friends Today</h1>
          <p className='mt-4 ms-1'>
            <span className='fw-bold'>Join DiscoFriends and start matching with people in seconds.</span> No more wasting time in servers you don't fit in with or trying to hold a conversation in a sea of messages.
          </p>
          <p>Just one-on-one chatting.</p>
          <div className={s.nextButtonContainer}>
            <Link className={s.nextButton + ' text-decoration-none'} href='/join' onClick={_ => {
              reportEvent('JoinButtonClick_Home');
            }}>Join Now</Link>
          </div>
        </div>
        <div className='col-12 col-lg-6 sr-none'>
          <div style={{transform: 'scale(.8)', cursor: 'default', userSelect: 'none'}}>
            <UserListCard user={{age: 21, pronouns: 'He/Him', gender: 'Male', accountId: 1, username: 'Bill1234', displayName: 'Bill', description: 'Hey! I love dogs and hiking. I\'m also a big WoW guy. I\'m in uni for CS.', tags: [{displayTag: 'World of Warcraft'}, {displayTag: 'Dogs'}, {displayTag: 'Hiking'}, {displayTag: 'Food'}], socialMedia: [{type: 'Discord', displayString: 'User#0000'}], avatar: {imageUrl: '/dog-256.jpg'}}} />

          </div>
        </div>
      </div>
    </div>
  </div>
}