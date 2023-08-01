import styles from './userListCard.module.css';
import Tags from "./tags";
import Accounts from "./accounts";
import PersonalInfo from "./personalInfo";
import Buttons from "./buttons";
export default function UserListCard(props) {
  const imageUrl = (props.user && props.user.avatar && props.user.avatar.imageUrl + "?size=256") || '/user_placeholder.png';
  return <div className={styles.card}>
    <div className={styles.cardPadding} />
    <div className={styles.cardHeader}>
      <div className={styles.cardHeaderItem}>
        <div className={styles.imageWrapper}>
          <img className={styles.image + ' sr-none'} src={imageUrl} />
        </div>
      </div>
      <div className={styles.cardHeaderItem + ' ms-4'}>
        <h3 className={styles.displayName}>{props.user.displayName}</h3>
        <p className={styles.username}>@{props.user.username}</p>
        <PersonalInfo age={props.user.age} gender={props.user.gender} pronouns={props.user.pronouns} />
      </div>
    </div>
    <div className={styles.cardInner}>

      <p className={styles.heading + ' text-uppercase'}>Description</p>
      <p className={styles.description}>{props.user.description || '-'}</p>

      <p className={styles.heading + ' text-uppercase'}>Tags</p>
      <Tags tags={props.user.tags} />

      <p className={styles.heading + ' text-uppercase mt-3'}>Accounts</p>
      <Accounts socialMedia={props.user.socialMedia} />

      {props.hideButtons ? null : <Buttons accountId={props.user.accountId} accept={props.accept} decline={props.decline} />}
    </div>
  </div>
}