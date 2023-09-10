import UserListCard from "../userListCard";
import s from './home.module.css';
import Link from "next/link";
import reportEvent from "../../events/eventBase";
import MatrixSpaces from "../matrixSpaces";

export default function Home() {
  return <div className='min-vh-100'>
    <div className={s.main}>
      <div className='container'>
        <div className='row mt-4'>
          <div className='col-12'>
            <h1 className={s.header + ' text-center'}>Make Matrix Friends Today</h1>
            <div className={s.homeDescription}>
              <p className={'mt-4 ms-1 text-center'}>
                <span className='fw-bold'>Join DiscoFriends and start matching with people in seconds.</span> Create friendships with one user at a time through our <Link href={'/list'} className='fw-bold'>Matching System</Link>, or join a <Link href={'/spaces'} className='fw-bold'>Space</Link> to meet many people at once.
              </p>
            </div>
            <p className='mt-4 ms-1 text-center'>

            </p>
            <div className={s.nextButtonContainer}>
              <Link className={s.nextButton + ' text-decoration-none'} href='/join' onClick={_ => {
                reportEvent('JoinButtonClick_Home');
              }}>Join Now</Link>
            </div>
          </div>
        </div>
      </div>
    </div>
    <img className={s.homeWave} src='/wave-1.svg' />
    <div className='container'>
      <div className='row'>
        <div className='col-12'>
          <h3 className={s.headerSpaces}>Recent Spaces</h3>
        </div>
      </div>
    </div>
    <MatrixSpaces header={false} limit={20} showMore={true} />
  </div>
}