using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Cluster.Queries.GetClusterStats
{
    public class GetClusterStatsQuery: IRequest<QueryResult<ClusterStats>>
    {
    }
}
