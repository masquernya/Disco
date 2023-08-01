import s from './tutorial.module.css';
export default function TutorialPage({description}) {
  return <div className={s.page}>
    {description}
  </div>
}