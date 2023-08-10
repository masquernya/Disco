import Head from 'next/head';
import ResetPassword from "../../../components/resetPassword";
import {useRouter} from "next/router";

export default function Home() {
  const router = useRouter();
  const token = router.query.token;
  if (!token)
    return null;

  return <div className='container min-vh-100'>
    <Head>
      <title>Reset Password</title>
    </Head>

    <div className='row'>
      <div className='col-12 col-lg-6 mx-auto'>
        <ResetPassword token={token} verified={true} />
      </div>
    </div>
  </div>
}