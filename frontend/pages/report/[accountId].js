import {useRouter} from "next/router";
import ReportForm from "../../components/reportForm";

export default function ReportAccount({props}) {
  const router = useRouter();
  const accountId = parseInt(router.query.accountId, 10);
  if (!accountId || !Number.isSafeInteger(accountId))
    return null;
  return <ReportForm accountId={accountId} />;
}