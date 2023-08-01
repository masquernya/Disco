import SignUp from "../components/signup";
import Head from "next/head";
import Script from "next/script";

export default function Join(props) {
  return <div className='container min-vh-100'>
    <Head>
      <title>Join DiscoFriends</title>
    </Head>
    <div className='row mt-4'>
      <div className='col-12 col-lg-6 mx-auto'>
        <SignUp />
      </div>
    </div>
  </div>
}