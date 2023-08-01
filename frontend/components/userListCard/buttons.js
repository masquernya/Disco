import s from './userListCard.module.css';
import Link from "next/link";
export default function Buttons({accountId,accept,decline}) {
  return <>
    <div className={s.buttonsContainer}>
      <button className={s.buttonAccept + ' text-uppercase'} onClick={() => {
        if (accept)
          accept();
      }}>Accept</button>
      <button className={s.buttonDecline + ' text-uppercase'} onClick={() => {
        if (decline)
          decline();
      }}>Decline</button>
    </div>
    <div className={s.reportContainer}>
      <Link href={`/report/${accountId}`} className={s.reportLabel}>
          Report
      </Link>
    </div>
  </>
}