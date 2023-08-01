import {useState} from "react";
import Users from "../components/users";
import Head from "next/head";

export default function List() {
 return <>
  <Head>
   <title>List - DiscoFriends</title>
  </Head>
  <Users />
 </>
}